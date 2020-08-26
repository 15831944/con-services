﻿using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.SiteModels.Interfaces.Events;

namespace VSS.TRex.SiteModels.GridFabric.Events
{
  /// <summary>
  /// Contains all relevant information detailing a mutating change event made to a site model that effects the metadata and
  /// other information either directly contained within a site model (eg: project extents, cell size etc) or referenced by it
  /// (eg: machines, target event lists, designs, site models etc)
  /// </summary>
  public class SiteModelAttributesChangedEvent : BaseRequestResponse, ISiteModelAttributesChangedEvent
  {
    private const byte VERSION_NUMBER = 1;

    public Guid SiteModelID { get; set; } = Guid.Empty;
    public bool ExistenceMapModified { get; set; }
    public bool DesignsModified { get; set; }
    public bool SurveyedSurfacesModified { get; set; }
    public bool CsibModified { get; set; }
    public bool MachinesModified { get; set; }
    public bool MachineTargetValuesModified { get; set; }
    public bool MachineDesignsModified { get; set; }
    public bool ProofingRunsModified { get; set; }
    public bool AlignmentsModified { get; set; }
    public bool SiteModelMarkedForDeletion { get; set; }


    /// <summary>
    /// A serialized bit mask sub grid tree representing the set of sub grids that have been changed in a
    /// mutating event on the site model such as TAG file processing
    /// </summary>
    public byte[] ExistenceMapChangeMask { get; set;  }

    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteGuid(SiteModelID);
      writer.WriteBoolean(ExistenceMapModified);
      writer.WriteBoolean(DesignsModified);
      writer.WriteBoolean(SurveyedSurfacesModified);
      writer.WriteBoolean(CsibModified);
      writer.WriteBoolean(MachinesModified);
      writer.WriteBoolean(MachineTargetValuesModified);
      writer.WriteBoolean(MachineDesignsModified);
      writer.WriteBoolean(ProofingRunsModified);
      writer.WriteByteArray(ExistenceMapChangeMask);
      writer.WriteBoolean(AlignmentsModified);
      writer.WriteBoolean(SiteModelMarkedForDeletion);
    }

    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        SiteModelID = reader.ReadGuid() ?? Guid.Empty;
        ExistenceMapModified = reader.ReadBoolean();
        DesignsModified = reader.ReadBoolean();
        SurveyedSurfacesModified = reader.ReadBoolean();
        CsibModified = reader.ReadBoolean();
        MachinesModified = reader.ReadBoolean();
        MachineTargetValuesModified = reader.ReadBoolean();
        MachineDesignsModified = reader.ReadBoolean();
        ProofingRunsModified = reader.ReadBoolean();
        ExistenceMapChangeMask = reader.ReadByteArray();
        AlignmentsModified = reader.ReadBoolean();
        SiteModelMarkedForDeletion = reader.ReadBoolean();
      }
    }
  }
}
