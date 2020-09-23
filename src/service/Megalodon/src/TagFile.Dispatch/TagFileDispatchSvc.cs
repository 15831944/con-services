﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TagFiles.Common;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Gateway.Common.Abstractions;
using VSS.WebApi.Common;

namespace MegalodonSvc
{
  public class TagFileDispatchSvc : IHostedService
  {
    private readonly ILogger<TagFileDispatchSvc> _log;
    private readonly ITPaaSApplicationAuthentication _authn;
    private readonly IConfigurationStore _config;
    private readonly ITRexTagFileProxy _serviceProxy;
    private FileSystemWatcher fileSystemWatcher;
    private readonly string _path;
    private TagFileTransfer tagFileTransfer = new TagFileTransfer();

    public TagFileDispatchSvc(ITPaaSApplicationAuthentication authn, ILoggerFactory logFactory, IConfigurationStore config, ITRexTagFileProxy serviceProxy)
    {
      _log = logFactory.CreateLogger<TagFileDispatchSvc>();
      _authn = authn;
      _config = config;
      _serviceProxy = serviceProxy;
      _path = Path.Combine(_config.GetValueString("InstallFolder"), TagConstants.TAGFILE_FOLDER);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
      MonitorDirectory(_path);
      return Task.CompletedTask;
    }


    public Task StopAsync(CancellationToken cancellationToken)
    {
      if (fileSystemWatcher != null)
      {
        fileSystemWatcher.EnableRaisingEvents = false;
      }
      return Task.CompletedTask;
    }

    private Task ScanAndUpload()
    {
      return Task.WhenAll(GetFilenames().Select(f => UploadFile(f).ContinueWith(t => {
        if (!t.IsFaulted && t.Result != null)
        {
          if (t.Result.Code == 0)
          {
            _log.LogInformation($"#Result# Tagfile successfully sent. Deleting Tagfile {f}");
            File.Delete(f);
          }
          else
            _log.LogError($"#Result# Tagfile cannot be sent. {f} {t.Result.Message}");
        }
        else
        {
          if (t.Result == null)
          {
            _log.LogInformation($"#Result# (NullResult) Tagfile successfully sent. Deleting Tagfile {f}");
            File.Delete(f);
          }
          else
            _log.LogWarning($"#Result# Can not submit Tagfile {f}. {t.Result.Message}");
        }
      })));
    }

    private async Task<ContractExecutionResult> UploadFile(string filename)
    {
      var prodHost = _config.GetValueString("production-host") ?? TagConstants.DEFAULT_HOST;
      var prodBase = _config.GetValueString("production-base") ?? TagConstants.DEFAULT_BASE;

      _log.LogInformation($"Uploading Tagfile {filename}. Host:{prodHost}, Base:{prodBase}");

      var fileData = File.ReadAllBytes(filename);
      var compactionTagFileRequest = new CompactionTagFileRequest
      {
        FileName = Path.GetFileName(filename),
        Data = fileData
      };

      try
      {
        var res = await tagFileTransfer.SendTagFile(compactionTagFileRequest, prodHost, prodBase);
        _log.LogInformation($"Tagfile upload result: {res.Message}");
        return res;
      }
      catch (Exception e)
      {
        _log.LogError(e, $"Can not submit file {filename}");
        return new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, e.Message); 
      }
    }

    private IEnumerable<string> GetFilenames() => Directory.GetFiles(_path, "*.tag").Where(f => File.GetCreationTimeUtc(f) > DateTime.UtcNow.AddMinutes(TagConstants.NEGATIVE_MINUTES_AGED_TAGFILES));

    private void MonitorDirectory(string path)

    {
      fileSystemWatcher = new FileSystemWatcher();
      fileSystemWatcher.Path = path;
      fileSystemWatcher.Created += async (s, e) => await FileSystemWatcher_Created(s,e);
      fileSystemWatcher.EnableRaisingEvents = true;
    }

    private Task FileSystemWatcher_Created(object sender, FileSystemEventArgs e)
    {
      return ScanAndUpload();
    }
  }
}
