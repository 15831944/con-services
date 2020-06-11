﻿namespace VSS.TRex.Types
{
    /// <summary>
    /// MachineType describes the individual type of machines that are recognized by name
    /// </summary>
    public enum MachineType : byte
  {
        Unknown = 0,
        Dozer = 23,
        Grader = 24,
        Excavator = 25,
        MotorScraper = 26,
        TowedScraper = 27,
        CarryAllScraper = 28,
        RubberTyreDozer = 29,
        WheelLoader = 30,
        WheelTractor = 31,
        SoilCompactor = 32,
        ForemansTruck = 33,
        Generic = 34,
        MillerPlaner = 36,
        BackhoeLoader = 37,
        AsphaltCompactor = 39,
        KerbandGutter = 41,
        AsphaltPaver = 42,
        FourDrumLandfillCompactor = 43,
        Trimmer = 44,
        ConcretePaver = 45,
        CutterSuctionDredge = 70,
        BargeMountedExcavator = 71
  }
}
