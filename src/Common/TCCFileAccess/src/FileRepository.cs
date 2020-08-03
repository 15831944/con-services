﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.Http;
using VSS.MasterData.Proxies;
using VSS.TCCFileAccess.Models;

namespace VSS.TCCFileAccess
{
  /// <summary>
  /// File access class to talk to TCC. This class is thread safe 
  /// </summary>
  /// <seealso cref="IFileRepository" />
  /// TODO add resiliency with Polly. Include CheckForInvalidTicket into resiliency logic.
  public class FileRepository : IFileRepository
  {
    public string tccBaseUrl { get; }
    public string tccUserName { get; }
    public string tccPassword { get; }
    public string tccOrganization { get; }

    private const string INVALID_TICKET_ERRORID = "NOT_AUTHENTICATED";
    private const string INVALID_TICKET_MESSAGE = "You have not authenticated, use login action";

    //Reinvalidate tcc ticket every 30 min
    private static DateTime lastLoginTimestamp;


    private readonly ILogger<FileRepository> Log;
    private readonly ILoggerFactory logFactory;
    private readonly IConfigurationStore configStore;
    private readonly IHttpClientFactory _httpClientFactory;

    private static string ticket = string.Empty;
    private static readonly object ticketLockObj = new object();

    /// <summary>
    /// The file cache - contains byte array for PNGs as a value and a full filename (path to the PNG) as a key. 
    /// It should persist across session so static. This class is thread-safe.
    /// Caching the tile files improves performance as downloading files from TCC is expensive.
    /// </summary>
    private static readonly MemoryCache fileCache = new MemoryCache(new MemoryCacheOptions
    {
      /* Todo: Determine if there is a mititgation action we can do in .Net COre 3+
       * Note: This flag was removed in .Net Core 3.0, per https://docs.microsoft.com/en-us/dotnet/core/compatibility/2.2-3.1
       *
       * Reason for change
       * Automatically compacting the cache caused problems. To avoid unexpected behavior, the cache should only be compacted when needed.
       * 
       * Recommended action
       * To compact the cache, downcast to MemoryCache and call Compact when needed.
       */
      // CompactOnMemoryPressure = true,

      ExpirationScanFrequency = TimeSpan.FromMinutes(10)
    });

    /// <summary>
    /// The cache lookup class to support fast keys lookup in cache by filename, not the full path to the PNG as there are a lot of tiles per a file.
    /// It folder structure in TCC is /customeruid/projectuid/filename_generatedsuffix$.DXF_Tiles$/zoom/ytile/xtile.png
    /// </summary>
    private readonly CacheLookup cacheLookup = new CacheLookup();

    private string Ticket
    {
      get
      {
        lock (ticketLockObj)
        {
          if (!string.IsNullOrEmpty(ticket) && lastLoginTimestamp >= DateTime.UtcNow.AddMinutes(-30)) return ticket;
          ticket = Login().Result;
          lastLoginTimestamp = DateTime.UtcNow;
          return ticket;
        }
      }
    }

    public FileRepository(IConfigurationStore configuration, ILoggerFactory logger, IHttpClientFactory httpClientFactory)
    {
      tccBaseUrl = configuration.GetValueString("TCCBASEURL");
      tccUserName = configuration.GetValueString("TCCUSERNAME");
      tccPassword = configuration.GetValueString("TCCPWD");
      tccOrganization = configuration.GetValueString("TCCORG");
      if (string.IsNullOrEmpty(tccBaseUrl) || string.IsNullOrEmpty(tccUserName) ||
          string.IsNullOrEmpty(tccPassword) || string.IsNullOrEmpty(tccOrganization))
      {
        throw new Exception("Missing environment variable TCCBASEURL, TCCUSERNAME, TCCPWD or TCCORG");
      }
      if (!tccBaseUrl.ToLower().StartsWith("http"))
      {
        throw new Exception($"Invalid TCC URL {tccBaseUrl}");
      }
      logFactory = logger;
      Log = logger.CreateLogger<FileRepository>();
      configStore = configuration;
      Log.LogInformation($"TCCBASEURL={tccBaseUrl}");
      _httpClientFactory = httpClientFactory;
    }

    public async Task<List<Organization>> ListOrganizations()
    {
      Log.LogDebug("ListOrganizations");
      List<Organization> orgs = null;
      try
      {
        var fileSpaceParams = new GetFileSpacesParams();
        var filespacesResult = await ExecuteRequest<GetFileSpacesResult>(Ticket, "GetFileSpaces", fileSpaceParams);

        if (filespacesResult != null)
        {
          if (filespacesResult.success)
          {
            if (filespacesResult.filespaces != null)
            {
              if (filespacesResult.filespaces.Any())
              {
                orgs =
                  filespacesResult.filespaces.Select(
                      filespace =>
                        new Organization
                        {
                          filespaceId = filespace.filespaceId,
                          shortName = filespace.orgShortname,
                          orgId = filespace.orgId,
                          orgDisplayName = filespace.orgDisplayName,
                          orgTitle = filespace.shortname
                        })
                    .ToList();
              }
            }
            else
            {
              Log.LogWarning("No organizations returned from ListOrganizations");
              orgs = new List<Organization>();
            }
          }
          else
          {
            CheckForInvalidTicket(filespacesResult, "ListOrganizations");
          }
        }
        else
        {
          Log.LogError("Null result from ListOrganizations");
        }
      }
      catch (Exception ex)
      {
        Log.LogError(ex, "Failed to get list of TCC organizations");
      }
      return orgs;
    }

    public Task<PutFileResponse> PutFile(Organization org, string path, string filename, Stream contents, long sizeOfContents)
    {
      Log.LogDebug("PutFile: org={0}", org.shortName);

      return PutFileEx(org.filespaceId, path, filename, contents, sizeOfContents);
    }

    public async Task<bool> PutFile(string filespaceId, string path, string filename, Stream contents,
      long sizeOfContents)
    {
      var result = await PutFileEx(filespaceId, path, filename, contents, sizeOfContents);

      if (!result.success)
        CheckForInvalidTicket(result, "PutFile");

      return result.success;
    }

    public async Task<PutFileResponse> PutFileEx(string filespaceId, string path, string filename, Stream contents,
      long sizeOfContents)
    {
      Log.LogDebug("PutFileEx: filespaceId={0}, fullName={1} {2}", filespaceId, path, filename);

      //NOTE: for this to work in TCC the path must exist otherwise TCC either gives an error or creates the file as the folder name
      var sendFileParams = new PutFileRequest
      {
        filespaceid = filespaceId,
        path = path,
        replace = true,
        commitUpload = true,
        filename = filename
      };
      if (string.IsNullOrEmpty(tccBaseUrl))
        throw new Exception("Configuration Error - no TCC url specified");

      var gracefulClient = new GracefulWebRequest(logFactory, configStore, _httpClientFactory);
      var (requestString, headers) = FormRequest(sendFileParams, "PutFile");

      headers.Add("X-File-Name", WebUtility.UrlEncode(filename));
      headers.Add("X-File-Size", sizeOfContents.ToString());
      headers.Add("X-FileType", "");
      headers.Add("Content-Type", ContentTypeConstants.ApplicationOctetStream);

      var result = default(PutFileResponse);
      try
      {
        result = await gracefulClient.ExecuteRequest<PutFileResponse>(requestString, contents, headers, HttpMethod.Put);
      }
      catch (WebException webException)
      {
        using (var response = webException.Response)
        {
          Log.LogWarning(
            $"Can not execute request TCC request with error {webException.Status} and {webException.Message}. {GetStringFromResponseStream(response)}");
        }
      }
      catch (Exception exception)
      {
        Log.LogWarning($"TCC request failed: {exception.Message}");
      }

      return result;
    }

    /// <summary>
    /// Gets the file. The resulting stream should be disposed after read completed
    /// </summary>
    public Task<Stream> GetFile(Organization org, string fullName, int retries = 0) => GetFileEx(org.filespaceId, fullName, retries);

    /// <summary>
    /// Gets the file. The resulting stream should be disposed after read completed
    /// </summary>
    public async Task<Stream> GetFile(string filespaceId, string fullName, int retries = 0) => await GetFileEx(filespaceId, fullName, retries);

    private async Task<Stream> GetFileEx(string filespaceId, string fullName, int retries)
    {
      Log.LogDebug($"{nameof(GetFileEx)}: filespaceId={filespaceId}, fullName={fullName}, retries={retries}");

      byte[] file;
      var cacheable = TCCFile.FileCacheable(fullName);

      if (cacheable)
      {
        Log.LogDebug("Trying to extract from cache {0} with cache size {1}", fullName, fileCache.Count);

        if (fileCache.TryGetValue(fullName, out file))
        {
          Log.LogDebug("Serving TCC tile request from cache {0}", fullName);

          if (file.Length == 0)
          {
            Log.LogDebug("Serving TCC tile request from cache empty tile");
            return null;
          }

          return new MemoryStream(file);
        }
      }

      var getFileParams = new GetFileParams
      {
        filespaceid = filespaceId,
        path = fullName
      };

      if (string.IsNullOrEmpty(tccBaseUrl))
      {
        throw new Exception("Configuration Error - no TCC url specified");
      }

      var gracefulClient = new GracefulWebRequest(logFactory, configStore, _httpClientFactory);
      var (requestString, headers) = FormRequest(getFileParams, "GetFile");

      try
      {
        if (!cacheable)
        {
          using (var responseStream = await (await gracefulClient.ExecuteRequestAsStreamContent(requestString, HttpMethod.Get, headers, retries: 0)).ReadAsStreamAsync())
          {
            responseStream.Position = 0;
            file = new byte[responseStream.Length];
            responseStream.Read(file, 0, file.Length);
            return new MemoryStream(file);
          }
        }

        using (var responseStream = await (await gracefulClient.ExecuteRequestAsStreamContent(requestString, HttpMethod.Get, headers, retries: retries)).ReadAsStreamAsync())
        {
          Log.LogDebug("Adding TCC tile request to cache {0}", fullName);
          responseStream.Position = 0;
          file = new byte[responseStream.Length];
          responseStream.Read(file, 0, file.Length);
          fileCache.Set(fullName, file, DateTimeOffset.MaxValue);
          Log.LogDebug("About to extract file name for {0}", fullName);
          var baseFileName = TCCFile.ExtractFileNameFromTileFullName(fullName);
          Log.LogDebug("Extracted file name is {0}", baseFileName);
          cacheLookup.AddFile(baseFileName, fullName);
          return new MemoryStream(file);
        }
      }
      catch (WebException webException)
      {
        using (var response = webException.Response)
        {
          Log.LogWarning(
            $"Can not execute request TCC request with error {webException.Status} and {webException.Message}. {GetStringFromResponseStream(response)}");
        }
        //let's cache the response anyway but for a limited time
        fileCache.Set(fullName, new byte[0], DateTimeOffset.UtcNow.AddHours(12));
      }
      catch (Exception e)
      {
        Log.LogWarning(e, "Can not execute request TCC response.");
      }
      return null;
    }

    public async Task<bool> MoveFile(Organization org, string srcFullName, string dstFullName)
    {
      Log.LogDebug("MoveFile: org={0} {1}, srcFullName={2}, dstFullName={3}", org.shortName, org.filespaceId,
        srcFullName, dstFullName);
      try
      {
        var dstPath = dstFullName.Substring(0, dstFullName.LastIndexOf("/", StringComparison.Ordinal)) + "/";
        if (!await FolderExists(org.filespaceId, dstPath))
        {
          var resultCreate = await MakeFolder(org.filespaceId, dstPath);
          if (!resultCreate)
          {
            Log.LogError("Can not create folder for org {0} folder {1}", org.shortName,
              dstPath);
            return false;
          }
        }

        var renParams = new RenParams
        {
          filespaceid = org.filespaceId,
          path = srcFullName,
          newfilespaceid = org.filespaceId,
          newPath = dstFullName,
          merge = false,
          replace = true
        };
        var renResult = await ExecuteRequest<RenResult>(Ticket, "Ren", renParams);
        if (renResult != null)
        {
          if (renResult.success || renResult.errorid.Contains("INVALID_OPERATION_FILE_IS_LOCKED"))
            return true;

          CheckForInvalidTicket(renResult, "MoveFile");
        }
        else
        {
          Log.LogError("Null result from MoveFile for org {0} file {1}", org.shortName, srcFullName);
        }
      }
      catch (Exception ex)
      {
        Log.LogError("Failed to move TCC file for org {0} file {1}: {2}", org.shortName, srcFullName,
          ex.Message);
      }
      return false;
    }


    public Task<bool> CopyFile(string filespaceId, string srcFullName, string dstFullName)
    {
      return CopyFile(filespaceId, filespaceId, srcFullName, dstFullName);
    }

    public async Task<bool> CopyFile(string srcFilespaceId, string dstFilespaceId, string srcFullName, string dstFullName)
    {
      Log.LogDebug(
        $"CopyFile: srcFilespaceId={srcFilespaceId}, srcFilespaceId={srcFilespaceId}, dstFilespaceId={dstFilespaceId} srcFullName={srcFullName}, dstFullName={dstFullName}");
      try
      {
        var dstPath = dstFullName.Substring(0, dstFullName.LastIndexOf("/", StringComparison.Ordinal)) + "/";
        if (!await FolderExists(dstFilespaceId, dstPath))
        {
          var resultCreate = await MakeFolder(dstFilespaceId, dstPath);
          if (!resultCreate)
          {
            Log.LogError("Can not create folder for filespaceId {dstFilespaceId} folder {dstPath}");
            return false;
          }
        }

        var copyParams = new CopyParams
        {
          filespaceid = srcFilespaceId,
          path = srcFullName,
          newfilespaceid = dstFilespaceId,
          newPath = dstFullName,
          merge = false,
          replace = true//Not sure if we want true or false here
        };
        var copyResult = await ExecuteRequest<ApiResult>(Ticket, "Copy", copyParams);
        if (copyResult != null)
        {
          if (copyResult.success || copyResult.errorid.Contains("INVALID_OPERATION_FILE_IS_LOCKED"))
            return true;

          CheckForInvalidTicket(copyResult, "CopyFile");
        }
        else
        {
          Log.LogError($"Null result from CopyFile for filespaceId {srcFilespaceId} file {srcFullName}");
        }
      }
      catch (Exception ex)
      {
        Log.LogError($"Failed to copy TCC file for srcFilespaceId={srcFilespaceId}, srcFilespaceId={srcFilespaceId}, dstFilespaceId={dstFilespaceId} srcFullName={srcFullName}, dstFullName={dstFullName} error:{ex.Message}");
      }
      return false;
    }

    public async Task<DirResult> GetFolders(Organization org, DateTime lastModifiedUTC, string path)
    {
      Log.LogDebug("GetFolders: org={0} {1}, lastModfiedUTC={2}, path={3}", org.shortName, org.filespaceId,
        lastModifiedUTC, path);
      try
      {
        //Get list of folders one level down from path
        var dirParams = new DirParams
        {
          filespaceid = org.filespaceId,
          path = path,
          recursive = false,
          filterfolders = true,
        };
        var dirResult = await ExecuteRequest<DirResult>(Ticket, "Dir", dirParams);
        if (dirResult != null)
        {
          if (dirResult.success)
            return dirResult;
          CheckForInvalidTicket(dirResult, "GetFolders");
        }
        else
        {
          Log.LogError("Null result from GetFolders for org {0}", org.shortName);
        }
      }
      catch (Exception ex)
      {
        Log.LogError(ex, "Failed to get list of TCC folders");
      }
      return null;
    }

    public async Task<DirResult> GetFileList(string filespaceId, string path, string fileMasks = null)
    {
      Log.LogDebug("GetFileList: filespaceId={0}, path={1}, fileMask={2}", filespaceId, path, fileMasks);
      try
      {
        //Get list of files one level down from path
        var dirParams = new DirParams
        {
          filespaceid = filespaceId,
          path = path,
          recursive = false,
          filterfolders = false,
        };
        if (!string.IsNullOrEmpty(fileMasks))
          dirParams.filemasks = fileMasks;
        var dirResult = await ExecuteRequest<DirResult>(Ticket, "Dir", dirParams);
        if (dirResult != null)
        {
          if (dirResult.success)
            return dirResult;
          CheckForInvalidTicket(dirResult, "GetFileList");
        }
        else
        {
          Log.LogError("Null result from GetFileList for filespaceId {0}", filespaceId);
        }
      }
      catch (Exception ex)
      {
        Log.LogError(ex, "Failed to get list of TCC files");
      }
      return null;
    }

    public async Task<DateTime> GetLastChangedTime(string filespaceId, string path)
    {
      Log.LogDebug("GetLastChangedTime: filespaceId={0}, path={1}", filespaceId, path);

      try
      {
        var lastDirChangeParams = new LastDirChangeParams
        {
          filespaceid = filespaceId,
          path = path,
          recursive = true
        };
        var lastDirChangeResult =
          await ExecuteRequest<LastDirChangeResult>(Ticket, "LastDirChange", lastDirChangeParams);
        if (lastDirChangeResult != null)
        {
          if (lastDirChangeResult.success)
          {
            return lastDirChangeResult.lastUpdatedDateTime;
          }
          CheckForInvalidTicket(lastDirChangeResult, "GetLastChangedTime");
        }
        else
        {
          Log.LogError("Null result from GetLastChangedTime for filespaceId={0}, path={1}", filespaceId,
            path);
        }
      }
      catch (Exception ex)
      {
        Log.LogError(ex, "Failed to get last time tag files added to folder");
      }
      return DateTime.MinValue;
    }

    public Task<bool> FolderExists(string filespaceId, string folder) => PathExists(filespaceId, folder);

    public Task<bool> FileExists(string filespaceId, string filename)
    {
      if (TCCFile.FileCacheable(filename))
      {
        if (fileCache.TryGetValue(filename, out _))
        {
          return Task.FromResult(true);
        }
      }

      return PathExists(filespaceId, filename);
    }

    private async Task<bool> PathExists(string filespaceId, string path)
    {
      Log.LogDebug("Searching for file or folder {0}", path);
      try
      {
        var getFileAttrParams = new GetFileAttributesParams
        {
          filespaceid = filespaceId,
          path = path
        };

        var getFileAttrResult =
          await ExecuteRequestWithAllowedError<GetFileAttributesResult>(Ticket, "GetFileAttributes", getFileAttrParams);
        if (getFileAttrResult != null)
        {
          if (getFileAttrResult.success)
          {
            return true;
          }
          CheckForInvalidTicket(getFileAttrResult, "PathExists", false); //don't log "file does not exist"
          return getFileAttrResult.success;
        }
      }
      catch (Exception ex)
      {
        Log.LogError(ex, "Failed to get TCC file attributes");
      }
      return false;
    }


    public Task<bool> DeleteFolder(string filespaceId, string path) => DeleteFileEx(filespaceId, path, true);

    public Task<bool> DeleteFile(string filespaceId, string fullName) => DeleteFileEx(filespaceId, fullName, false);

    public async Task<bool> DeleteFileEx(string filespaceId, string fullName, bool isFolder)
    {
      Log.LogDebug("DeleteFileEx: filespaceId={0}, fullName={1}", filespaceId, fullName);
      try
      {
        var deleteParams = new DeleteFileParams
        {
          filespaceid = filespaceId,
          path = fullName,
          recursive = isFolder
        };
        var deleteResult = await ExecuteRequest<DeleteFileResult>(Ticket, "Del", deleteParams);
        if (deleteResult != null)
        {
          if (deleteResult.success)
          {
            return true;
          }
          CheckForInvalidTicket(deleteResult, "DeleteFile");
          return deleteResult.success;
        }
      }
      catch (Exception ex)
      {
        Log.LogError(ex, "Failed to delete file");
      }
      return false;
    }

    public async Task<bool> MakeFolder(string filespaceId, string path)
    {
      Log.LogDebug("MakeFolder: filespaceId={0}, path={1}", filespaceId, path);
      try
      {
        var mkDirParams = new MkDir
        {
          filespaceid = filespaceId,
          path = path,//WebUtility.UrlEncode(path),
          force = true
        };

        var mkDirResult = await ExecuteRequest<MkDirResult>(Ticket, "MkDir", mkDirParams);
        if (mkDirResult != null)
        {
          if (mkDirResult.success)
          {
            return true;
          }
          CheckForInvalidTicket(mkDirResult, "MakeFolder");
          return mkDirResult.success;
        }
      }
      catch (Exception ex)
      {
        Log.LogError(ex, "Failed to make directory");
      }
      return false;
    }

    private async Task<string> Login()
    {
      Log.LogInformation("Logging in to TCC: user={0}, org={1}", tccUserName, tccOrganization);
      try
      {
        var loginParams = new LoginParams
        {
          username = tccUserName,
          orgname = tccOrganization,
          password = tccPassword,
          mode = "noredirect",
          forcegmt = true
        };
        var loginResult = await ExecuteRequest<LoginResult>(ticket, "Login", loginParams);
        if (loginResult != null)
        {
          if (loginResult.success)
            return loginResult.ticket;

          Log.LogError("Failed to login to TCC: errorId={0}, reason={1}", loginResult.errorid,
            loginResult.reason);
        }
        else
        {
          Log.LogError("Null result from Login");
        }
        return null;
      }
      catch (Exception ex)
      {
        Log.LogError(ex, "Failed to login to TCC");
        return null;
      }
    }

    private void CheckForInvalidTicket(ApiResult result, string what, bool logWarning = true)
    {
      //Check for expired/invalid ticket
      if (!result.success)
      {
        if (result.errorid == INVALID_TICKET_ERRORID && result.message == INVALID_TICKET_MESSAGE)
        {
          ticket = null;
        }
        else if (logWarning)
        {
          Log.LogWarning("{0} failed: errorid={1}, message={2}", what, result.errorid, result.message);
        }
      }
    }

    private (string, IHeaderDictionary) FormRequest(object request, string endpoint, string token = null)
    {
      var headers = new HeaderDictionary();

      var properties = request.GetType()
                               .GetRuntimeFields()
                               .Where(p => p.GetValue(request) != null).Select(p => new { p.Name, Value = p.GetValue(request) })
                               .ToDictionary(d => d.Name, v => v.Value.ToString());

      properties.Add("ticket", token ?? Ticket);

      using (var encodedContent = new FormUrlEncodedContent(properties))
      {
        var requestString = $"{tccBaseUrl}/tcc/{endpoint}?" + encodedContent.ReadAsStringAsync().Result;

        return (requestString, headers);
      }
    }

    private async Task<T> ExecuteRequest<T>(string token, string contractPath, object requestData)
    {
      if (string.IsNullOrEmpty(tccBaseUrl))
        throw new Exception("Configuration Error - no TCC url specified");

      var gracefulClient = new GracefulWebRequest(logFactory, configStore, _httpClientFactory);
      var (requestString, headers) = FormRequest(requestData, contractPath, token);

      headers.Add("Content-Type", ContentTypeConstants.ApplicationJson);
      var result = default(T);
      try
      {
        result = await gracefulClient.ExecuteRequest<T>(requestString, method: HttpMethod.Get, customHeaders: headers);
      }
      catch (WebException webException)
      {
        using (var response = webException.Response)
        {
          Log.LogWarning(
            $"Can not execute request TCC request with error {webException.Status} and {webException.Message}. {GetStringFromResponseStream(response)}");
        }
      }
      catch (Exception e)
      {
        Log.LogWarning(e, "Can not execute request TCC response.");
      }
      return result;
    }

    private async Task<T> ExecuteRequestWithAllowedError<T>(string token, string contractPath, object requestData)
      where T : ApiResult, new()
    {
      const string FILE_DOES_NOT_EXIST_ERROR =
        "{\"errorid\":\"FILE_DOES_NOT_EXIST\",\"message\":\"File does not exist\",\"success\":false}";

      if (string.IsNullOrEmpty(tccBaseUrl))
        throw new Exception("Configuration Error - no TCC url specified");

      var gracefulClient = new GracefulWebRequest(logFactory, configStore, _httpClientFactory);
      var (requestString, headers) = FormRequest(requestData, contractPath, token);

      headers.Add("Content-Type", ContentTypeConstants.ApplicationJson);
      var result = default(T);
      try
      {
        result = await gracefulClient.ExecuteRequest<T>(requestString, method: HttpMethod.Get, customHeaders: headers, retries: 0, suppressExceptionLogging: true);
      }
      catch (WebException webException)
      {
        using (var response = webException.Response)
        {
          var tccError = GetStringFromResponseStream(response);

          if (tccError == FILE_DOES_NOT_EXIST_ERROR)
          {
            var tccErrorResult = JsonConvert.DeserializeObject<ApiResult>(FILE_DOES_NOT_EXIST_ERROR);

            result = new T
            {
              success = tccErrorResult.success,
              errorid = tccErrorResult.errorid,
              message = tccErrorResult.message
            };
          }
          else
          {
            Log.LogWarning(
              $"Can not execute request TCC request with error {webException.Status} and {webException.Message}. {tccError}");
          }
        }
      }
      catch (Exception e)
      {
        Log.LogWarning(e, "Can not execute request TCC response");
      }

      return result;
    }

    private static string GetStringFromResponseStream(WebResponse response)
    {
      using (var readStream = response.GetResponseStream())
      {
        if (readStream == null)
        {
          return string.Empty;
        }

        using (var reader = new StreamReader(readStream, Encoding.UTF8))
        {
          return reader.ReadToEnd();
        }
      }
    }

    #region Tile Rendering

    public async Task<string> CreateFileJob(string filespaceId, string path)
    {
      Log.LogDebug("CreateFileJob: filespaceId={0}, path={1}", filespaceId, path);
      try
      {
        var jobParams = new CreateFileJobParams
        {
          filespaceid = filespaceId,
          path = path,//WebUtility.UrlEncode(path),
          type = "GEOFILEINFO",
          forcerender = false
        };
        var jobResult = await ExecuteRequest<CreateFileJobResult>(Ticket, "CreateFileJob", jobParams);
        if (jobResult != null)
        {
          if (jobResult.success)
          {
            //This assumes that we're about to (re)generate tiles for the file
            //therefore clear the cache for this file if cached tiles exist.
            var filenames = cacheLookup.RetrieveCacheKeysExact(path);
            if (filenames != null)
            {
              Log.LogDebug($"Removing files for {path} from cachelookup and dropping cache");
              filenames.ForEach(s => fileCache.Remove(s));
              filenames.Clear();
              cacheLookup.DropCacheKeys(path);
            }

            return jobResult.jobId;
          }
          CheckForInvalidTicket(jobResult, "CreateFileJob");
          return null;
        }
      }
      catch (Exception ex)
      {
        Log.LogError(ex, "Failed to create file job");
      }
      return null;
    }

    public async Task<CheckFileJobStatusResult> CheckFileJobStatus(string jobId)
    {
      Log.LogDebug("CheckFileJobStatus: jobId={0}", jobId);
      try
      {
        var statusParams = new CheckFileJobStatusParams
        {
          jobid = jobId
        };
        var statusResult = await ExecuteRequest<CheckFileJobStatusResult>(Ticket, "CheckFileJobStatus", statusParams);
        if (statusResult != null)
        {
          if (statusResult.success)
            return statusResult;

          CheckForInvalidTicket(statusResult, "CheckFileJobStatus");
          return null;
        }
      }
      catch (Exception ex)
      {
        Log.LogError(ex, "Failed to check file job status");
      }
      return null;
    }

    public async Task<GetFileJobResultResult> GetFileJobResult(string fileId)
    {
      Log.LogDebug("GetFileJobResult: fileId={0}", fileId);
      try
      {
        var resultParams = new GetFileJobResultParams
        {
          fileid = fileId
        };
        var resultResult = await ExecuteRequest<GetFileJobResultResult>(Ticket, "GetFileJobResult", resultParams);
        //TODO: Check if graceful request works here. It's a stream of bytes returned which we want to process as text
        //(see ApiCallBase.ProcessResponseAsText)
        return resultResult;

      }
      catch (Exception ex)
      {
        Log.LogError(ex, "Failed to get file job result");
      }
      return null;
    }

    public async Task<string> ExportToWebFormat(string srcFilespaceId, string srcPath,
      string dstFilespaceId, string dstPath, int zoomLevel)
    {
      Log.LogDebug("ExportToWebFormat: srcFilespaceId={0}, srcPath={1}, dstFilespaceId={2}, dstPath={3}, zoomLevel={4}",
        srcFilespaceId, srcPath, dstFilespaceId, dstPath, zoomLevel);
      try
      {
        var exportParams = new ExportToWebFormatParams
        {
          sourcefilespaceid = srcFilespaceId,
          sourcepath = srcPath,
          destfilespaceid = dstFilespaceId,
          destpath = dstPath,
          format = "GoogleMaps",
          numzoomlevels = 1,
          maxzoomlevel = zoomLevel,
          imageformat = "png"
        };
        var exportResult = await ExecuteRequest<ExportToWebFormatResult>(Ticket, "ExportToWebFormat", exportParams);
        if (exportResult != null)
        {
          if (exportResult.success)
            return exportResult.jobId;

          CheckForInvalidTicket(exportResult, "ExportToWebFormat");
          return null;
        }
      }
      catch (Exception ex)
      {
        Log.LogError(ex, "Failed to export to web format");
      }
      return null;
    }

    public async Task<string> CheckExportJob(string jobId)
    {
      Log.LogDebug("CheckExportJob: jobId={0}", jobId);
      try
      {
        var checkParams = new CheckExportJobParams
        {
          jobid = jobId
        };
        var checkResult = await ExecuteRequest<CheckExportJobResult>(Ticket, "CheckExportJob", checkParams);
        if (checkResult != null)
        {
          if (checkResult.success)
            return checkResult.status;

          CheckForInvalidTicket(checkResult, "CheckExportJob");
          return null;
        }
      }
      catch (Exception ex)
      {
        Log.LogError(ex, "Failed to check export job status");
      }
      return null;
    }

    #endregion
  }
}
