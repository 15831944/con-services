﻿namespace VSS.TRex.GridFabric.Models.Servers
{
  /// <summary>
  /// Defines names of various role that servers can occupy in the grid
  /// </summary>
  public static class ServerRoles
  {
    /// <summary>
    /// The name of the attribute added to a node attributes to record its role
    /// </summary>
    public const string ROLE_ATTRIBUTE_NAME = "Role";

    /// <summary>
    /// The 'PSNode' role, meaning the server is a part of sub grid clustered processing engine
    /// </summary>
    public const string PSNODE = "PSNode";

    /// <summary>
    /// The 'ASNode', application service, role, meaning the server is a part of sub grid clustered processing engine
    /// </summary>
    public const string ASNODE = "ASNode";

    /// <summary>
    /// The 'ASNode' profiling role, meaning the server supports profiling operations
    /// </summary>
    public const string ASNODE_PROFILER = "ASNode-Profiler";

    /// <summary>
    /// The generic 'ASNode', application service, client role
    /// </summary>
    public const string ASNODE_CLIENT = "ASNodeClient";

    /// <summary>
    /// A server responsible for processing TAG files into the production data models
    /// </summary>
    public const string TAG_PROCESSING_NODE = "TagProc";

    /// <summary>
    /// A server responsible for processing TAG files into the production data models
    /// </summary>
    public const string TAG_PROCESSING_NODE_CLIENT = "TagProcClient";

    /// <summary>
    /// A server responsible for rendering tiles from production data
    /// </summary>
    public const string TILE_RENDERING_NODE = "TileRendering";

    /// <summary>
    /// A server responsible for producing patches of sub grids for Patch requests
    /// </summary>
    public const string PATCH_REQUEST_ROLE = "Patches";

    /// <summary>
    /// A server responsible for computing various analytic queries, such as cut fill statistics, from production data
    /// </summary>
    public const string ANALYTICS_NODE = "Analytics";

    /// <summary>
    /// A server responsible for producing elevation sub grid information from design and surveyed surface topology models (TTMs)
    /// </summary>
    public const string DESIGN_PROFILER = "DesignProfiler";

    /// <summary>
    /// A server responsible for decimating production elevation data into a TIN surface for export
    /// </summary>
    public const string TIN_SURFACE_EXPORT_ROLE = "TINSurfaceExport";

    /// <summary>
    /// A server responsible for assembling reporting queries, such as grid report, from production data
    /// </summary>
    public const string REPORTING_ROLE = "Reporting";

    /// <summary>
    /// A server responsible for rendering quantized mesh tiles from production data
    /// </summary>
    public const string QNANTIZED_MESH_NODE = "QuantizedMesh";

    /// <summary>
    /// A server responsible for rebulding projects in TRex. 
    /// </summary>
    public const string PROJECT_REBUILDER_ROLE = "ProjectRebuilder";

    /// <summary>
    /// A server responsible for coordinating actions that mutate state within the mutable grid with
    /// the intention that those changes are projected to the immutable grid
    /// </summary>
    public const string DATA_MUTATION_ROLE = "DataMutator";

    /// <summary>
    /// The server wants to receive notifications regarding changes to site models
    /// </summary>
    public const string RECEIVES_SITEMODEL_CHANGE_EVENTS = "Receives-SiteModelAttributesChangedEvents";

    /// <summary>
    /// The server wants to receive notifications regarding changes to designs
    /// </summary>
    public const string RECEIVES_DESIGN_CHANGE_EVENTS = "Receives-DesignChangeEvents";
  }
}
