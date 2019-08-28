﻿using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApi.Models.MapHandling;
using VSS.Productivity3D.WebApiModels.Notification.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.WebApi.Models.Notification.Executors
{
  /// <summary>
  /// Processes the request to delete a file.
  /// Action taken depends on the file type.
  /// </summary>
  public class DeleteFileExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public DeleteFileExecutor()
    {
      ProcessErrorCodes();
    }

    /// <summary>
    /// Populates ContractExecutionStates with Production Data Server error messages.
    /// </summary>
    /// 
    protected sealed override void ProcessErrorCodes()
    {
      RaptorResult.AddErrorMessages(ContractExecutionStates);
    }
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      try
      {
        var request = CastRequestObjectTo<ProjectFileDescriptor>(item);
        var fileType = request.FileType;

        log.LogDebug($"FileType is: {fileType}");

        if (fileType == ImportedFileType.DesignSurface ||
            fileType == ImportedFileType.Alignment ||
            fileType == ImportedFileType.Linework)
        {
          var suffix = FileUtils.GeneratedFileSuffix(fileType);
          //Delete generated files
          var deletePRJFile = DeleteGeneratedFile(request.ProjectId.Value, request.File, suffix, FileUtils.PROJECTION_FILE_EXTENSION);
          var deleteHAFile = DeleteGeneratedFile(request.ProjectId.Value, request.File, suffix, FileUtils.HORIZONTAL_ADJUSTMENT_FILE_EXTENSION);

          await Task.WhenAll(deletePRJFile, deleteHAFile);

          bool success = deletePRJFile.Result && deleteHAFile.Result;

          if (fileType != ImportedFileType.Linework)
            success = success && await DeleteGeneratedFile(request.ProjectId.Value, request.File, suffix, FileUtils.DXF_FILE_EXTENSION);

          if (!success)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
                "Failed to delete generated files"));
          }
          //Delete tiles 
          var generatedName = FileUtils.GeneratedFileName(request.File.FileName, suffix, FileUtils.DXF_FILE_EXTENSION);
          await tileGenerator.DeleteDxfTiles(request.ProjectId.Value, generatedName, request.File).ConfigureAwait(false);
        }


        //If surveyed surface, delete it in Raptor
        if (fileType == ImportedFileType.SurveyedSurface)
        {
          log.LogDebug("Discarding ground surface file in Raptor");
          bool importedFileDiscardByNextGenFileIdResult =
            raptorClient.DiscardGroundSurfaceFileDetails(request.ProjectId.Value, request.FileId);
          bool importedFileDiscardByLegacyFileIdResult = true;

          if (request.LegacyFileId.HasValue)
          {
            importedFileDiscardByLegacyFileIdResult =
              raptorClient.DiscardGroundSurfaceFileDetails(request.ProjectId.Value, request.LegacyFileId.Value);
          }

          // one or the other should be deleted
          if (!importedFileDiscardByNextGenFileIdResult && !importedFileDiscardByLegacyFileIdResult)
          {
            var whichOneFailed = importedFileDiscardByNextGenFileIdResult
              ? $"LegacyFileId {request.LegacyFileId}"  :  $"nextGenId {request.FileId}";
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
                $"Failed to discard ground surface file by {whichOneFailed}"));
          }
        }

        return new ContractExecutionResult(ContractExecutionStatesEnum.ExecutedSuccessfully, "Delete file notification successful");        
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }

    }

    /// <summary>
    /// Delete a generated file associated with the specified file
    /// </summary>
    /// <param name="projectId">The id of the project to which the file belongs</param>
    /// <param name="fileDescr">The original file</param>
    /// <param name="suffix">The suffix applied to the file name to get the generated file name</param>
    /// <param name="extension">The file extension of the generated file</param>
    /// <returns>True if the file is successfully deleted, false otherwise</returns>
    private async Task<bool> DeleteGeneratedFile(long projectId, FileDescriptor fileDescr, string suffix, string extension)
    {
      string generatedName = FileUtils.GeneratedFileName(fileDescr.FileName, suffix, extension);
      log.LogDebug("Deleting generated file {0}", generatedName);
      var fullName = string.Format("{0}/{1}", fileDescr.Path, generatedName);
      if (await fileRepo.FileExists(fileDescr.FilespaceId, fullName))
      {
        if (!await fileRepo.DeleteFile(fileDescr.FilespaceId, fullName))
        {
          log.LogWarning("Failed to delete file {0} for project {1}", generatedName, projectId);
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
              "Failed to delete associated file " + generatedName));
        }
        return true;
      }
      return true;//TODO: Is this what we want if file not there?
    }

 
  }
}
