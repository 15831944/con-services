﻿namespace VSS.TRex.Types
{
    /// <summary>
    ///  The types of data held in an FS file: Spatial Directory and segment information, events and ProductionDataXML
    /// </summary>
    public enum FileSystemStreamType
  {
        SubGridSegment,
        SubGridDirectory,
        Events,
        ProductionDataXML,
        SubGridExistenceMap,
        CoordinateSystemCSIB,
        SurveyedSurfaces,
        Designs,
        Machines,
        MachineDesigns,
        MachineDesignNames,
        ProofingRuns,
        Alignments,
        SubGridVersionMap
    }
}
