﻿using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.GridFabric.Servers.Client;

namespace VSS.TRex.Exports.Servers.Client
{
  public class TINSurfaceExportRequestServer : ApplicationServiceServer
  {
      /// <summary>
      /// Default no-arg constructor that creates a server with the default Application Service role and the specialize tile rendering role.
      /// </summary>
      public TINSurfaceExportRequestServer() : base(new[] { ServerRoles.TIN_SURFACE_EXPORT_ROLE })
      {
      }

      public TINSurfaceExportRequestServer(string[] roles) : base(roles)
      {
      }

      /// <summary>
      /// Creates a new instance of a Patch request server
      /// </summary>
      /// <returns></returns>
      public static TINSurfaceExportRequestServer NewInstance(string[] roles)
      {
        return new TINSurfaceExportRequestServer(roles);
      }
    }
}
