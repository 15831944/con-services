﻿namespace ProductionDataSvc.AcceptanceTests.Models
{
    /// <summary>
    /// The list of 'display modes' that Raptor understans in the context of rendering WMS tiles and other operations
    /// </summary>
    public enum DisplayMode
    {
        /// <summary>
        /// The elevation above the grid coordinate datum of the project coordinate system.
        /// </summary>
        Height = 0,

        /// <summary>
        /// Raw CCV/CMV (Caterpillar Compaction Value/Compaction Meter Value) values recorded by compaction machine systems
        /// </summary>
        CCV = 1,

        /// <summary>
        /// CCV values expressed as a percentage between the raw measured CCV value and either the configured target CCV on the machine or a global override target CCV value.
        /// </summary>
        CCVPercent = 2,

        /// <summary>
        /// Radio latency reported by the machine systems, where the latency refers to the age of the RTK corrections induced by the radio network transmission latency between the RTK base station and the machine.
        /// </summary>
        Latency = 3,

        /// <summary>
        /// The number of passes measured within the top most layer of material identified by layer analysis.
        /// </summary>
        PassCount = 4,

        /// <summary>
        /// Resonance meter value indicating how close the reactive force of the ground against the compactive energy being directed into it by the offset-mass vibrating drum is to causing the drum to bounce.
        /// </summary>
        RMV = 5,

        /// <summary>
        /// The reported vibratory drum frequency on a compactor
        /// </summary>
        Frequency = 6,

        /// <summary>
        /// The reported vibratory drum amplitude on a compactor
        /// </summary>
        Amplitude = 7,

        /// <summary>
        /// The cut or fill calculated from the comparison of two surfaces which may be a mixture of filtered machine originated production data and design surfaces.
        /// </summary>
        CutFill = 8,

        /// <summary>
        /// The reported soil moisture content from a moisture sensor on a soil compactor
        /// </summary>
        Moisture = 9,

        /// <summary>
        /// Analysed summary temperature information from recorded temperatures values from asphalt compactors.
        /// </summary>
        TemperatureSummary = 10,

        /// <summary>
        /// The reported GPSMode values from a machine
        /// </summary>
        GPSMode = 11,

        /// <summary>
        /// Analysed raw CCV summary information from recorded compaction values from asphalt and soil compactors.
        /// </summary>
        CCVSummary = 12,

        /// <summary>
        /// Analysed raw CCV percentage summary information from recorded compaction values from asphalt and soil compactors.
        /// </summary>
        CCVPercentSummary = 13, // This is a synthetic display mode for CCV summary

        /// <summary>
        /// Analysed passcount summary information from asphalt and soil compactors.
        /// </summary>
        PassCountSummary = 14, // This is a synthetic display mode for Pass Count summary

        /// <summary>
        /// Information indication only where data exists within a project.
        /// </summary>
        CompactionCoverage = 15, // This ia a synthetic display mode for Compaction Coverage

        /// <summary>
        /// Information indicating where in the project volume calculations occurred and in which areas there was no volumetric difference between the comparative surfaces.
        /// </summary>
        VolumeCoverage = 16, // This is a synthetic display mode for Volumes Coverage

        /// <summary>
        /// Raw Machine Drive Power values recorded by compaction machine systems
        /// </summary>
        MDP = 17,

        /// <summary>
        /// MDP values expressed as a percentage between the raw measured MDP value and either the configured target MDP on the machine or a global override target CCV value.
        /// </summary>
        MDPSummary = 18,

        /// <summary>
        /// Analysed raw MDP summary information from recorded compaction values from asphalt and soil compactors.
        /// </summary>
        MDPPercent = 19,

        /// <summary>
        /// Analysed raw MDP percentage summary information from recorded compaction values from asphalt and soil compactors.
        /// </summary>
        MDPPercentSummary = 20, // This is a synthetic display mode for MDP summary

        /// <summary>
        /// An analysis of a cell in terms of the layers derived from profile analysis of information within it
        /// </summary>
        CellProfile = 21,

        /// <summary>
        /// An analysis of a cell in terms of the layers derived from profile analysis of information within it, and the cell passes contained in the analysed layers
        /// </summary>
        CellPasses = 22,

        /// <summary>
        /// Machine Speed valus recorded by compaction machine systems
        /// </summary>
        MachineSpeed = 23,

        /// <summary>
        /// The CCV percent change calculates change of the CCV in % between current and previous CCV % over target. Normal filtering rules are applied.
        /// </summary>
        CCVPercentChange = 24,

        /// <summary>
        /// Target thickness summary overlay. Renders cells with three colors - above target, within target, below target. Target value shall be specified in the request.
        /// </summary>
        TargetThicknessSummary = 25,

        /// <summary>
        /// Target speed summary overlay. Renders cells with three colors - over target range, within target range, lower target range. Target range values shall be specified in the request.
        /// </summary>
        TargetSpeedSummary = 26,

        /// <summary>
        /// The CCV change calculates change of the CCV in % between current and previous CCV absolute values. Normal filtering rules are applied.
        /// </summary>
        CMVChange = 27
    }
}