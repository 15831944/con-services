﻿using Apache.Ignite.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.GridFabric.Grids;

namespace VSS.VisionLink.Raptor.GridFabric.Events
{
    /// <summary>
    /// Responsible for sending a notification that the attributes of a site model have changed
    /// By definition, all server and client nodes should react to this message
    /// </summary>
    public static class SiteModelAttributesChangedEventSender
    {
        /// <summary>
        /// Notify all nodes in the grid a site model has changed attributes
        /// </summary>
        /// <param name="siteModelID"></param>
        public static void ModelAttributesChanged(IIgnite ignite, long siteModelID)
        {
            ignite?.GetMessaging().Send(new SiteModelAttributesChangedEvent()
            {
                SiteModelID = siteModelID
            });
        }
    }
}
