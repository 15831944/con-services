﻿using Apache.Ignite.Core;
using Apache.Ignite.Core.Cluster;
using Apache.Ignite.Core.Resource;
using Apache.Ignite.Core.Services;
using System;
using System.Collections.Generic;
using VSS.VisionLink.Raptor.Designs;
using VSS.VisionLink.Raptor.Geometry;
using VSS.VisionLink.Raptor.GridFabric.NodeFilters;
using VSS.VisionLink.Raptor.GridFabric.Grids;
using VSS.VisionLink.Raptor.Services.Surfaces;
using VSS.VisionLink.Raptor.Surfaces;

namespace VSS.VisionLink.Raptor.Services.Surfaces
{
    /// <summary>
    /// Class responsible for deploying the add surveyed surface service
    /// </summary>
    public class SurveyedSurfaceServiceProxy
    {
        /// <summary>
        /// The cluster wide name of the deployed service
        /// </summary>
        public const string ServiceName = "AddSurveyedSurface";

        /// <summary>
        /// Services interface for the clustergroup projection
        /// </summary>
        private IServices services = null;

        /// <summary>
        /// The proxy to the deploy service
        /// </summary>
        private ISurveyedSurfaceService proxy = null;

        /// <summary>
        /// No-arg constructor that instantiates the Ignitre instance, cluster, service and proxy members
        /// </summary>
        public SurveyedSurfaceServiceProxy()
        {
            IIgnite _ignite = Ignition.TryGetIgnite(RaptorGrids.RaptorGridName());

            // Get an instance of IServices for the cluster group.
            services = _ignite.GetCluster().GetServices();
        }

        public void Deploy()
        {
            // Attempt to cancel any previously deployed service
            services.Cancel(ServiceName);

            services.Deploy(new ServiceConfiguration()
            {
                Name = ServiceName,
                Service = new SurveyedSurfaceService(),
                TotalCount = 1,
                MaxPerNodeCount = 1,
                NodeFilter = new PSNodeRoleBasedNodeFilter()
            });

            proxy = services.GetServiceProxy<ISurveyedSurfaceService>(ServiceName);
        }

        /// <summary>
        /// Invoke proxy for calling the add surveyed surface service
        /// </summary>
        /// <param name="SiteModelID"></param>
        /// <param name="designDescriptor"></param>
        /// <param name="asAtDate"></param>
        public void Invoke_Add(long SiteModelID, DesignDescriptor designDescriptor, DateTime asAtDate, BoundingWorldExtent3D extents)
        {
            proxy.Add(SiteModelID, designDescriptor, asAtDate.Date, extents);
        }

        public bool Invoke_Remove(long SiteModelID, long SurveyedSurfaceID)
        {
            return proxy.Remove(SiteModelID, SurveyedSurfaceID);
        }

        /// <summary>
        /// Invoke proxy for calling the list surveyed surface service
        /// </summary>
        /// <param name="SiteModelID"></param>
        public SurveyedSurfaces Invoke_List(long SiteModelID)
        {
            return proxy.List(SiteModelID);
        }
    }
}
