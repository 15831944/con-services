﻿using System.Net;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.ResultHandling.Coords;
using VSS.Productivity3D.Productivity3D.Models;

namespace VSS.TRex.Gateway.Common.Executors.Coords
{
  /// <summary>
  /// Processes the request to get coordinate system definition data (CSIB) as string.
  /// </summary>
  public class CSIBExecutor : BaseExecutor
  {
    public CSIBExecutor(IConfigurationStore configStore, ILoggerFactory logger, IServiceExceptionHandler exceptionHandler)
      : base(configStore, logger, exceptionHandler)
    { }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public CSIBExecutor()
    { }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      var request = item as ProjectID;

      if (request == null)
        ThrowRequestTypeCastException<ProjectID>();

      var siteModel = GetSiteModel(request.ProjectUid);

      var csib = siteModel.CSIB();

      if (csib == string.Empty)
        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
          $"The project does not have Coordinate System definition data. Project UID: {siteModel.ID}"));

      return new CSIBResult(csib);
    }
  }
}
