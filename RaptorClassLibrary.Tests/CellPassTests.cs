﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Raptor.Types;
using System.IO;
using VSS.VisionLink.Raptor;
using VSS.VisionLink.Raptor.Cells;

namespace VSS.VisionLink.Raptor.RaptorClassLibrary.Tests
{
    [TestClass]
    public class CellPassTests
    {
        public static CellPass ATestCellPass()
        {
            return new CellPass()
            {
                Amplitude = 1,
                CCA = 2,
                CCV = 3,
                Frequency = 4,
                gpsMode = GPSMode.AutonomousPosition,
                halfPass = false,
                Height = 5,
                MachineID = 6,
                MachineSpeed = 7,
                MaterialTemperature = 8,
                MDP = 9,
                passType = PassType.Front,
                RadioLatency = 10,
                RMV = 11,
                SiteModelMachineIndex = 0,
                Time = new DateTime(2017, 1, 1, 12, 30, 0)
            };
        }

        public static CellPass ATestCellPass2()
        {
            return new CellPass()
            {
                Amplitude = 10,
                CCA = 20,
                CCV = 30,
                Frequency = 40,
                gpsMode = GPSMode.DGPS,
                halfPass = true,
                Height = 50,
                MachineID = 60,
                MachineSpeed = 70,
                MaterialTemperature = 80,
                MDP = 90,
                passType = PassType.Rear,
                RadioLatency = 100,
                RMV = 110,
                SiteModelMachineIndex = 0,
                Time = new DateTime(2017, 1, 1, 12, 45, 0)
            };
        }

        /// <summary>
        /// Test creation of a new cell pass with no non-null values specified
        /// </summary>
        [TestMethod]
        public void Test_CellPass_CreateNullPass()
        {
            CellPass cp = new CellPass();
            cp.Clear();

            Assert.IsTrue(
                cp.Amplitude == CellPass.NullAmplitude &&
                cp.CCA == CellPass.NullCCA &&
                cp.CCV == CellPass.NullCCV &&
                cp.Frequency == CellPass.NullFrequency &&
                cp.gpsMode == CellPass.NullGPSMode &&
                cp.halfPass == false &&
                cp.Height == CellPass.NullHeight &&
                cp.MachineID == CellPass.NullMachineID &&
                cp.MachineSpeed == CellPass.NullMachineSpeed &&
                cp.MaterialTemperature == CellPass.NullMaterialTemp &&
                cp.MDP == CellPass.NullMDP &&
                cp.passType == PassType.Front &&
                cp.RadioLatency == CellPass.NullRadioLatency &&
                cp.RMV == CellPass.NullRMV &&
                cp.SiteModelMachineIndex == short.MaxValue &&
                cp.Time == CellPass.NullTime,
                "Newly created/cleared CellPass does not contain all null values");
        }

        /// <summary>
        /// Test extraction of a machine ID and time as a pair returns the expected values
        /// </summary>
        [TestMethod]
        public void Test_CellPass_MachineIDAndTime()
        {
            CellPass cp = ATestCellPass();

            DateTime testTime = DateTime.Now;
            cp.MachineID = 100;
            cp.Time = testTime;

            long MachineID;
            DateTime Time;

            cp.MachineIDAndTime(out MachineID, out Time);
            Assert.IsTrue(MachineID == 100 && Time == testTime, "Machine ID and time are not the expected values");
        }

        /// <summary>
        /// Test setting fields for vide stae off crrect resets the appropriate fields
        /// </summary>
        [TestMethod]
        public void Test_CellPass_SetFieldsFroVibeStateOff()
        {
            CellPass cp = ATestCellPass();

            Assert.IsFalse(cp.CCV == CellPass.NullCCV ||
                           cp.RMV == CellPass.NullRMV ||
                           cp.Frequency == CellPass.NullFrequency ||
                           cp.Amplitude == CellPass.NullAmplitude,
                           "One or more fields for vibe state off are already null, compromising the test");

            cp.SetFieldsForVibeStateOff();

            Assert.IsTrue(cp.CCV == CellPass.NullCCV &&
                          cp.RMV == CellPass.NullRMV &&
                          cp.Frequency == CellPass.NullFrequency &&
                          cp.Amplitude == CellPass.NullAmplitude, 
                          "Appropriate fields for vibe state off are not null");
        }

        /// <summary>
        /// Test the Equality comparer funcions as expected
        /// </summary>
        [TestMethod]
        public void Test_CellPass_EqualityCheck_Self()
        {
            CellPass cp1;

            cp1 = ATestCellPass();
            Assert.IsTrue(cp1.Equals(cp1), "Equality check on self failed (returned true)");
        }

        /// <summary>
        /// Test the Equality comparer funcions as expected
        /// </summary>
        [TestMethod]
        public void Test_CellPass_EqualityCheck()
        {
            CellPass cp1;
            CellPass cp2;

            cp1 = ATestCellPass();
            cp2 = ATestCellPass();
            Assert.IsTrue(cp1.Equals(cp2), "Equality check on identical cell passes failed (returned false)");

            cp2 = ATestCellPass2();
            Assert.IsFalse(cp1.Equals(cp2), "Equality check on different cell passes failed (returned true)");
        }

        /// <summary>
        /// Test the Equality comparer funcions as expected
        /// </summary>
        [TestMethod]
        public void Test_CellPass_AssignCellPasses()
        {
            CellPass cp1 = ATestCellPass();
            CellPass cp2 = ATestCellPass2();

            Assert.IsFalse(cp1.Equals(cp2), "Equality check on different cell passes failed (returned true)");

            cp1.Assign(cp2);
            Assert.IsTrue(cp1.Equals(cp2), "Equality check on assigned cell passes failed (returned false)");
        }

        /// <summary>
        /// Test reading and writing binary format
        /// </summary>
        [TestMethod]
        public void Test_CellPass_BinaryReadWrite()
        {
            CellPass cp1 = ATestCellPass();
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            cp1.Write(bw);

            ms.Position = 0;
            BinaryReader br = new BinaryReader(ms);
            CellPass cp2 = new CellPass();

            cp2.Read(br);

            Assert.IsTrue(cp1.Equals(cp2), "Equality check on same cell passes failed after write then read (returned true)");

            // Check negative condition by writing and reading a second different cell pass then comparing the results of reading the two cell passes
            // to ensure they are different

            cp2 = ATestCellPass2();
            MemoryStream ms2 = new MemoryStream();
            BinaryWriter bw2 = new BinaryWriter(ms2);

            cp2.Write(bw2);

            ms2.Position = 0;
            BinaryReader br2 = new BinaryReader(ms2);

            cp2.Read(br2);

            Assert.IsFalse(cp1.Equals(cp2), "Equality check on different cell passes failed after write then read (returned true)");
        }

        /// <summary>
        /// Ensure the ToString() method returns non-null, and does not error out
        /// </summary>
        [TestMethod]
        public void Test_CellPass_ToString()
        {
            CellPass cp = ATestCellPass();

            Assert.IsFalse(String.IsNullOrEmpty(cp.ToString()), "ToString() result is null or empty");
        }
    }
}
