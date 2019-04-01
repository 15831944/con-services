﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Common;
using VSS.TRex.DI;
using VSS.TRex.Common.Utilities;
using VSS.TRex.TAGFiles.Executors;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.Validator
{
  public static class TagfileValidator
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

    /// <summary>
    /// Calls the TFA service via the nuget Proxy,
    ///    to validates licensing etc
    ///      and lookup assetId and projectId 
    /// </summary>
    /// <returns></returns>
    private static async Task<GetProjectAndAssetUidsResult> ValidateWithTfa(GetProjectAndAssetUidsRequest getProjectAndAssetUidsRequest)
    {
      Log.LogInformation($"#Progress# ValidateWithTFA. Calling TFA service to validate tagfile permissions:{JsonConvert.SerializeObject(getProjectAndAssetUidsRequest)}");
      var tfa = DIContext.Obtain<ITagFileAuthProjectProxy>();
      
      GetProjectAndAssetUidsResult tfaResult;
      try
      {
        var customHeaders = new Dictionary<string, string>();
        tfaResult = await tfa.GetProjectAndAssetUids(getProjectAndAssetUidsRequest, customHeaders).ConfigureAwait(false);
      }
      catch (Exception e)
      {
        return GetProjectAndAssetUidsResult.CreateGetProjectAndAssetUidsResult(string.Empty, string.Empty, (int) TRexTagFileResultCode.TfaException, e.Message);
      }
      
      return tfaResult;
    }

    private static string GetEC520SerialID(this string sValue)
    {
      const string EC520_SUFFIX = "YU";

      //Check if the value is a valid EC520 Serial ID else make field empty
      string EC520Serial = String.Empty;
      if (!string.IsNullOrEmpty(sValue))
      {
        if (sValue.Length > 2 && sValue.Substring(sValue.Length - 2, 2) == EC520_SUFFIX)
          EC520Serial = sValue;
      }
      return EC520Serial;
    }


    /// <summary>
    /// this needs to be public, only for unit tests
    /// </summary>
    /// <param name="tagDetail"></param>
    /// <param name="processor"></param>
    /// <returns></returns>
    public static async Task<GetProjectAndAssetUidsResult> CheckFileIsProcessible(TagFileDetail tagDetail, TAGFilePreScan preScanState)
    {
      /*
      Three different types of tagfile submission
      Type A: Automatic submission.
      This is where a tagfile comes in from a known org and the system works out what Asset and Project it belongs to. Licensing is checked

      Type B:  Manual submission.
      This is where the project is known and supplied as an override projectid value. The Asset is worked out via TFA service or assigned a JohnDoe id if not known. 
      Licensing is checked for manual subscription

      Type C: Override submission.
      This is where the ProjectId and AssetId is both supplied. It bypasses TFA service and providing the tagfile is valid, is processed straight into the project.
      This is not a typical submission but is handy for testing and in a situation where a known third party source other than NG could determine the AssetId and Project. Typical NG users could not submit via this method thus avoiding our license check. 
      */

      string EC520SerialID = GetEC520SerialID(preScanState.HardwareID);

      // Type C. Do we have what we need already (Most likely test tool submission)
      if (tagDetail.assetId != null && tagDetail.projectId != null)
        if (tagDetail.assetId != Guid.Empty && tagDetail.projectId != Guid.Empty)
          return GetProjectAndAssetUidsResult.CreateGetProjectAndAssetUidsResult(tagDetail.projectId.ToString(), tagDetail.assetId.ToString(), 0, "success");

      // Business rule for device type conversion
      var radioType = preScanState.RadioType == "torch" ? DeviceTypeEnum.SNM940 : DeviceTypeEnum.MANUALDEVICE; // torch device set to type 6

      if (preScanState.RadioSerial == string.Empty && Guid.Parse(tagDetail.tccOrgId) == Guid.Empty && tagDetail.projectId == Guid.Empty && EC520SerialID == string.Empty)
      {
        // this is a TFA code. This check is also done as a pre-check as the scenario is very frequent, to avoid the API call overhead.
        var message = "Must have either a valid TCCOrgID or RadioSerialNo or EC520SerialNo or ProjectUID";
        Log.LogWarning(message);
        return GetProjectAndAssetUidsResult.CreateGetProjectAndAssetUidsResult(tagDetail.projectId.ToString(), tagDetail.assetId.ToString(), 3037, message);
      }

      var tfaRequest = GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(
        tagDetail.projectId == null ? string.Empty : tagDetail.projectId.ToString(),
        (int)radioType, preScanState.RadioSerial, EC520SerialID, tagDetail.tccOrgId,
        MathUtilities.RadiansToDegrees(preScanState.SeedLatitude ?? 0),
        MathUtilities.RadiansToDegrees(preScanState.SeedLongitude ?? 0),
        preScanState.LastDataTime ?? DateTime.MinValue);

      var tfaResult = await ValidateWithTfa(tfaRequest).ConfigureAwait(false);

      Log.LogInformation($"#Progress# CheckFileIsProcessible. TFA GetProjectAndAssetUids returned for {tagDetail.tagFileName} tfaResult: {JsonConvert.SerializeObject(tfaResult)}");
      if (tfaResult?.Code == (int) TRexTagFileResultCode.Valid)
      {
        // if not overriding take TFA projectid
        if ((tagDetail.projectId == null || tagDetail.projectId == Guid.Empty) && (Guid.Parse(tfaResult.ProjectUid) != Guid.Empty))
        {
          tagDetail.projectId = Guid.Parse(tfaResult.ProjectUid);
        }

        // take what TFA gives us including an empty guid which is a JohnDoe
        tagDetail.assetId = tfaResult.AssetUid == string.Empty ? Guid.Empty :(Guid.Parse(tfaResult.AssetUid));

        // Check For JohnDoe machines. if you get a valid pass and no assetid it means it had a manual3dlicense
        if (tagDetail.assetId == Guid.Empty)
          tagDetail.IsJohnDoe = true; // JohnDoe Machine and OK to process
      }

      return tfaResult;
    }

    /// <summary>
    /// Inputs a tagfile for validation and asset licensing checks
    /// </summary>
    /// <param name="tagDetail"></param>
    /// <returns></returns>
    public static async Task<ContractExecutionResult> ValidSubmission(TagFileDetail tagDetail)
    {
      // Perform some Validation Checks 

      // get our settings
      var config = DIContext.Obtain<IConfigurationStore>();
      var minTagFileLength = config.GetValueInt("MIN_TAGFILE_LENGTH", Consts.kMinTagFileLengthDefault);
      var tfaServiceEnabled = config.GetValueBool("ENABLE_TFA_SERVICE", Consts.ENABLE_TFA_SERVICE);
      if (tagDetail.tagFileContent.Length <= minTagFileLength)
      {
        return new ContractExecutionResult((int) TRexTagFileResultCode.TRexInvalidTagfile, TRexTagFileResultCode.TRexInvalidTagfile.ToString());
      }

      TAGFilePreScan tagFilePresScan = new TAGFilePreScan();
      tagFilePresScan.Execute(new MemoryStream(tagDetail.tagFileContent));

      if (tagFilePresScan.ReadResult != TAGReadResult.NoError)
      {
        return new ContractExecutionResult((int)TRexTagFileResultCode.TrexTagFileReaderError, tagFilePresScan.ReadResult.ToString());
      }

      /*
      Old process of pre-scanning...
      // Now open tagfile and validate contents
      ISiteModel siteModel = DIContext.Obtain<ISiteModelFactory>().NewSiteModel();
      var machine = new Machine();
      var siteModelGridAggregator = new ServerSubGridTree(siteModel.ID);
      var machineTargetValueChangesAggregator = new ProductionEventLists(siteModel, MachineConsts.kNullInternalSiteModelMachineIndex);
      TAGProcessor processor = new TAGProcessor(siteModel, machine, siteModelGridAggregator, machineTargetValueChangesAggregator);
      TAGValueSink sink = new TAGVisionLinkPrerequisitesValueSink(processor);
      TAGReader reader = new TAGReader(new MemoryStream(tagDetail.tagFileContent));
      TAGFile tagFile = new TAGFile();

      TAGReadResult readResult = tagFile.Read(reader, sink);
      if (readResult != TAGReadResult.NoError)
      {
        return new ContractExecutionResult((int)TRexTagFileResultCode.TrexTagFileReaderError, readResult.ToString());
      }
       */

      // Tagfile contents are OK so proceed
      if (!tfaServiceEnabled) // allows us to bypass a TFA service
      {
        Log.LogWarning($"SubmitTAGFileResponse.ValidSubmission. EnableTFAService disabled. Bypassing TFS validation checks");
        if (tagDetail.projectId != Guid.Empty) // do we have what we need
        {
          if (tagDetail.assetId == null || tagDetail.assetId == Guid.Empty)
            tagDetail.IsJohnDoe = true;
          return new ContractExecutionResult((int)TRexTagFileResultCode.Valid);
        }
        
        // cannot process without asset and project id
        return new ContractExecutionResult((int)TRexTagFileResultCode.TRexBadRequestMissingProjectUid, "TRexTagFileResultCode.TRexBadRequestMissingProjectUid");         
      }

      // Contact TFA service to validate tagfile details
      var tfaResult = await CheckFileIsProcessible(tagDetail, tagFilePresScan).ConfigureAwait(false);
      return new ContractExecutionResult((int)tfaResult.Code, tfaResult.Message);
    }
  }
}
