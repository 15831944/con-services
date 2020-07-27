﻿using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.TagFileGateway.Common.Models.Sns;

namespace VSS.Productivity3D.TagFileGateway.Common.Executors
{
  public class TagFileSnsProcessExecutor : RequestExecutorContainer
  {
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      if (!(item is SnsPayload payload))
      {
        Logger.LogWarning($"{nameof(TagFileSnsProcessExecutor)} Invalid Request passed in. Expected {typeof(SnsPayload).Name} but got {(item == null ? "null" : item.GetType().Name)}");
        return ContractExecutionResult.ErrorResult("Invalid Request");
      }

      Logger.LogInformation($"{nameof(TagFileSnsProcessExecutor)} Sns message type: {payload.Type}, topic: {payload.TopicArn}");
      if (payload.Type == SnsPayload.SubscriptionType)
      {
        // Request for subscription
        Logger.LogInformation($"{nameof(TagFileSnsProcessExecutor)} SNS SUBSCRIPTION REQUEST: {payload.Message}, Subscription URL: '{payload.SubscribeURL}'");
        return new ContractExecutionResult();
      }
      if (payload.IsNotification)
      {
        // Got a tag file
        var tagFile = JsonConvert.DeserializeObject<SnsTagFile>(payload.Message);
        if (tagFile == null)
        {
          Logger.LogWarning($"{nameof(TagFileSnsProcessExecutor)} Could not convert to Tag File Model. JSON: {payload.Message}");
          return new ContractExecutionResult(1, "Failed to parse tag file model");
        }

        byte[] data;
        if (!string.IsNullOrEmpty(tagFile.DownloadUrl))
        {
          Logger.LogInformation($"{nameof(TagFileSnsProcessExecutor)} Tag file {tagFile.FileName} needs to be downloaded from : {tagFile.DownloadUrl}");
          var downloadTagFileData = await WebRequest.ExecuteRequestAsStreamContent(tagFile.DownloadUrl, HttpMethod.Get);
          await using var ms = new MemoryStream();
          await downloadTagFileData.CopyToAsync(ms);
          data = ms.ToArray();
          if (data.Length != tagFile.FileSize)
          {
            Logger.LogWarning($"{nameof(TagFileSnsProcessExecutor)} Downloaded data length {data.Length} is not equal to expected length {tagFile.FileSize}");
          }

          Logger.LogInformation($"{nameof(TagFileSnsProcessExecutor)} Downloaded tag file {tagFile.FileName}, total bytes: {data.Length}");
        }
        else
        {
          Logger.LogInformation($"{nameof(TagFileSnsProcessExecutor)} Tag file data is included in payload for file {tagFile.FileName}");
          data = tagFile.Data;
        }

        var request = new CompactionTagFileRequest {Data = data, FileName = tagFile.FileName, OrgId = tagFile.OrgId};

        Logger.LogInformation($"{nameof(TagFileSnsProcessExecutor)} Attempting to process sns tag file {tagFile.FileName}");
        var executor = Build<TagFileProcessExecutor>();
        executor.ArchiveOnInternalError = false;
        var result =  await executor.ProcessAsync(request);
        Logger.LogInformation($"{nameof(TagFileSnsProcessExecutor)} Got result {JsonConvert.SerializeObject(result)} for Tag file: {tagFile?.FileName}");

        return result;
      }

      Logger.LogWarning($"{nameof(TagFileSnsProcessExecutor)} Unknown SNS Type: {payload.Type} - not sure how to process");

      return new ContractExecutionResult(99, "Unknown SNS message");
    }
  }
}
