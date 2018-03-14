﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Cells;
using VSS.VisionLink.Raptor.TAGFiles.Classes;
using VSS.VisionLink.Raptor.TAGFiles.Classes.Sinks;
using VSS.VisionLink.Raptor.TAGFiles.Classes.States;
using VSS.VisionLink.Raptor.TAGFiles.Types;

namespace VSS.VisionLink.Raptor.Executors
{
    /// <summary>
    /// Executes a TAG file pre scan to extract the pieces of information useful for determining how 
    /// to process the TAG file information
    /// </summary>
    public class TAGFilePreScan
    {
        public double? SeedLatitude { get; set; } = null;
        public double? SeedLongitude { get; set; } = null;

        public int ProcessedEpochCount { get; set; } = 0;

        public string RadioType { get; set; } = String.Empty;
        public string RadioSerial { get; set; } = String.Empty;

        public byte MachineType { get; set; } = CellPass.MachineTypeNull;

        public string MachineID { get; set; } = String.Empty;
        public string HardwareID { get; set; } = String.Empty;


        public TAGReadResult ReadResult { get; set; } = TAGReadResult.NoError;

        /// <summary>
        /// Set the state of the executor to an initialised state
        /// </summary>
        private void Initialise()
        {
            SeedLatitude = null;
            SeedLongitude = null;
            ProcessedEpochCount = 0;

            RadioType = String.Empty;
            RadioSerial = String.Empty;

            MachineType = CellPass.MachineTypeNull;

            MachineID = String.Empty;
            HardwareID = String.Empty;

            ReadResult = TAGReadResult.NoError;
        }

        /// <summary>
        /// Default no-arg constructor. Sets up initial null state for information returned from a TAG file
        /// </summary>
        public TAGFilePreScan()
        {
            Initialise();
        }

        /// <summary>
        /// Fill out the local class properties with the information wanted from the TAG file
        /// </summary>
        /// <param name="Processor"></param>
        private void SetPublishedState(TAGProcessorPreScanState Processor)
        {
            SeedLatitude = Processor.LLHLat;
            SeedLongitude = Processor.LLHLon;
            ProcessedEpochCount = Processor.ProcessedEpochCount;
            RadioType = Processor.RadioType;
            RadioSerial = Processor.RadioSerial;

            MachineType = Processor.MachineType;

            MachineID = Processor.MachineID;
            HardwareID = Processor.HardwareID;
        }

        /// <summary>
        /// Execute the pre-scan operation on the TAG file, returning a booleam success result.
        /// Sets up local state detailing the prescan fields retried from the ATG file
        /// </summary>
        /// <param name="TAGData"></param>
        /// <returns></returns>
        public bool Execute(Stream TAGData)
        {
            try
            {
                Initialise();

                TAGProcessorPreScanState Processor = new TAGProcessorPreScanState();
                TAGValueSink Sink = new TAGVisionLinkPrerequisitesValueSink(Processor);
                TAGReader Reader = new TAGReader(TAGData);
                TAGFile TagFile = new TAGFile();

                ReadResult = TagFile.Read(Reader, Sink);

                if (ReadResult != TAGReadResult.NoError)
                {
                    return false;
                }

                SetPublishedState(Processor);
            }
            catch // (Exception E) // make sure any exception is trapped to return correct response to caller
            {
                return false;
            }

            return true;
        }
     
    }
}
