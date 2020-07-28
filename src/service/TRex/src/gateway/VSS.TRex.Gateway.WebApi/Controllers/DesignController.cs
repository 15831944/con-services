﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.ResultHandling.Designs;
using VSS.TRex.Alignments.Interfaces;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.Gateway.Common.Converters;
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.Gateway.Common.Executors.Design;
using VSS.TRex.Gateway.Common.Requests;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.TRex.Gateway.WebApi.Controllers
{
  /// <summary>
  /// Controller to get designs for a project.
  /// Create/Update/Delete endpoints use the mutable endpoint (at present VSS.TRex.Mutable.Gateway.WebApi)
  /// </summary>
  [Route("api/v1/design")]
  public class DesignController : BaseController
  {
    /// <inheritdoc />
    public DesignController(ILoggerFactory loggerFactory, IServiceExceptionHandler serviceExceptionHandler, IConfigurationStore configStore)
      : base(loggerFactory, loggerFactory.CreateLogger<DesignController>(), serviceExceptionHandler, configStore)
    { }

    /// <summary>
    /// Returns surface, alignment and surveyed surface designs
    ///    which is registered for a site model.
    /// If there are no designs the result will be an empty list.
    /// </summary>
    /// <param name="projectUid"></param>
    /// <param name="fileType"></param>
    /// <returns></returns>
    [HttpGet("get")]
    public DesignListResult GetDesignsForProject([FromQuery] Guid projectUid, [FromQuery] ImportedFileType? fileType)
    {
      Log.LogInformation($"{nameof(GetDesignsForProject)}: projectUid{projectUid} fileType: {fileType}");

      var designFileDescriptorList = new List<DesignFileDescriptor>();
      if (fileType != null && !(fileType == ImportedFileType.DesignSurface || fileType == ImportedFileType.SurveyedSurface || fileType == ImportedFileType.Alignment))
      { 
        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "File type must be DesignSurface, SurveyedSurface or Alignment"));
      }

      if ((DIContext.Obtain<ISiteModels>().GetSiteModel(projectUid)) == null)
      {
        return new DesignListResult { DesignFileDescriptors = designFileDescriptorList };
      }

      if (fileType == null || fileType == ImportedFileType.DesignSurface)
      {
        var designList = DIContext.Obtain<IDesignManager>().List(projectUid);
        if (designList != null)
        {
          designFileDescriptorList = designList.Select(designSurface =>
              AutoMapperUtility.Automapper.Map<DesignFileDescriptor>(designSurface))
            .ToList();
        }
      }

      if (fileType == null || fileType == ImportedFileType.SurveyedSurface)
      {
        var designSurfaceList = DIContext.Obtain<ISurveyedSurfaceManager>().List(projectUid);
        if (designSurfaceList != null)
        {
          designFileDescriptorList.AddRange(designSurfaceList.Select(designSurface =>
              AutoMapperUtility.Automapper.Map<DesignFileDescriptor>(designSurface))
            .ToList());
        }
      }

      if (fileType == null || fileType == ImportedFileType.Alignment)
      {
        var designAlignmentList = DIContext.Obtain<IAlignmentManager>().List(projectUid);
        if (designAlignmentList != null)
        {
          designFileDescriptorList.AddRange(designAlignmentList.Select(designAlignment =>
              AutoMapperUtility.Automapper.Map<DesignFileDescriptor>(designAlignment))
            .ToList());
        }
      }

      return new DesignListResult { DesignFileDescriptors = designFileDescriptorList };
    }

    /// <summary>
    /// Gets a list of design boundaries in GeoJson format from TRex database.
    /// </summary>
    /// <param name="projectUid">The site model/project unique identifier.</param>
    /// <param name="designUid">The design file unique identifier.</param>
    /// <param name="fileName">The design file name.</param>
    /// <param name="tolerance">The spacing interval for the sampled points. Setting to 1.0 will cause points to be spaced 1.0 meters apart.</param>
    /// <returns>Execution result with a list of design boundaries.</returns>
    [HttpGet("boundaries")]
    public Task<ContractExecutionResult> GetDesignBoundaries(
      [FromQuery] Guid projectUid, 
      [FromQuery] Guid designUid, 
      [FromQuery] string fileName, 
      [FromQuery] double? tolerance)
    {
      Log.LogInformation($"{nameof(GetDesignsForProject)}: projectUid:{projectUid}, designUid:{designUid}, fileName:{fileName}, tolerance: {tolerance}");

      const double BOUNDARY_POINTS_INTERVAL = 0.0;

      var designBoundariesRequest = new DesignBoundariesRequest(projectUid, designUid, fileName, tolerance ?? BOUNDARY_POINTS_INTERVAL);

      designBoundariesRequest.Validate();

      return WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainer
          .Build<DesignBoundariesExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .ProcessAsync(designBoundariesRequest));
    }

    /// <summary>
    /// Gets a list of design filter boundaries in GeoJson format from TRex database.
    /// </summary>
    /// <param name="projectUid"></param>
    /// <param name="designUid"></param>
    /// <param name="fileName"></param>
    /// <param name="startStation"></param>
    /// <param name="endStation"></param>
    /// <param name="leftOffset"></param>
    /// <param name="rightOffset"></param>
    /// <returns></returns>
    [HttpGet("filter/boundary")]
    public Task<ContractExecutionResult> GetDesignFilterBoundaries(
      [FromQuery] Guid projectUid, 
      [FromQuery] Guid designUid, 
      [FromQuery] string fileName, 
      [FromQuery] double startStation,
      [FromQuery] double endStation,
      [FromQuery] double leftOffset,
      [FromQuery] double rightOffset)
    {
      Log.LogInformation($"{nameof(GetDesignFilterBoundaries)}: projectUid:{projectUid}, designUid:{designUid}, fileName:{fileName}, " +
                         $"startStation: {startStation}, endStation: {endStation}, leftOffset: {leftOffset}, rightOffset: {rightOffset}");

      var designFilterBoundaryRequest = new DesignFilterBoundaryRequest(
        projectUid, 
        designUid, 
        fileName, 
        startStation,
        endStation,
        leftOffset,
        rightOffset);

      designFilterBoundaryRequest.Validate();

      return WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainer
          .Build<DesignFilterBoundaryExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .ProcessAsync(designFilterBoundaryRequest));
    }

    /// <summary>
    ///  Gets an alignment design station range from TRex database.
    /// </summary>
    /// <param name="projectUid"></param>
    /// <param name="designUid"></param>
    /// <returns></returns>
    [HttpGet("alignment/stationrange")]
    public Task<ContractExecutionResult> GetAlignmentStationRange([FromQuery] Guid projectUid, [FromQuery] Guid designUid)
    {
      Log.LogInformation($"{nameof(GetAlignmentStationRange)}: projectUid:{projectUid}, designUid:{designUid}");

      var alignmentStationRangeRequest = new DesignDataRequest(projectUid, designUid);

      alignmentStationRangeRequest.Validate();

      return WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainer
          .Build<AlignmentStationRangeExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .ProcessAsync(alignmentStationRangeRequest));
    }

    /// <summary>
    ///  Gets a model describing the geometry and station labeling for the master alignment in an alignment design
    /// </summary>
    /// <param name="projectUid"></param>
    /// <param name="designUid"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    [HttpGet("alignment/master/geometry")]
    public Task<ContractExecutionResult> GetAlignmentGeometryForRendering([FromQuery] Guid projectUid, [FromQuery] Guid designUid, [FromQuery] string fileName)
    {
      Log.LogInformation($"{nameof(GetAlignmentGeometryForRendering)}: projectUid:{projectUid}, designUid:{designUid}, fileName:{fileName}");

      var alignmentMasterGeometryRequest = new AlignmentDesignGeometryRequest(projectUid, designUid, fileName);

      alignmentMasterGeometryRequest.Validate();

      return WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainer
          .Build<AlignmentMasterGeometryExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .ProcessAsync(alignmentMasterGeometryRequest));
    }

    /*
    /// <summary>
    /// Produces a DXF file containing a rendering of the geometry and station labeling for the master alignment in an alignment design
    /// </summary>
    /// <param name="projectUid"></param>
    /// <param name="designUid"></param>
    /// <returns></returns>
    [HttpGet("alignment/master/render/dxf")]
    public Task<ContractExecutionResult> GetAlignmentGeometryRenderedToDXF([FromQuery] Guid projectUid, [FromQuery] Guid designUid)
    {
      Log.LogInformation($"{nameof(GetAlignmentStationRange)}: projectUid:{projectUid}, designUid:{designUid}");

      var alignmentStationRangeRequest = new DesignDataRequest(projectUid, designUid);

      alignmentStationRangeRequest.Validate();

      return WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainer
          .Build<AlignmentMasterGeometryExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .ProcessAsync(alignmentStationRangeRequest));
    }
    */
  }
}
