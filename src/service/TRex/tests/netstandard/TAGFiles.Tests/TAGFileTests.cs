﻿using System.IO;
using VSS.TRex.TAGFiles.Classes;
using VSS.TRex.TAGFiles.Classes.Processors;
using VSS.TRex.TAGFiles.Classes.Sinks;
using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Models;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace TAGFiles.Tests
{
    public class TAGFileTests : IClassFixture<DILoggingFixture>
    {
        [Fact]
        public void Test_TAGFile_Creation()
        {
            TAGFile file = new TAGFile();

            Assert.NotNull(file);
        }

        [Fact]
        public void Test_TAGFile_Read_Stream()
        {
            // Create the TAG file and reader classes
            TAGFile file = new TAGFile();
            using (var reader = new TAGReader(new FileStream(
              Path.Combine("TestData", "TAGFiles", "TestTAGFile-TAGFile-Read-Stream.tag"), FileMode.Open,
              FileAccess.Read)))
            {
              // Create the state and sink
              TAGProcessorStateBase stateBase = new TAGProcessorStateBase(); // Derivatives to construct later
              TAGValueSink sink = new TAGValueSink(stateBase);

              //Read the TAG file
              TAGReadResult result = file.Read(reader, sink);

              Assert.Equal(TAGReadResult.NoError, result);
            }
        }

        [Fact]
        public void Test_TAGFile_Read_File()
        {
            // Create the TAG file and reader classes
            TAGFile file = new TAGFile();

            // Create the state and sink
            TAGProcessorStateBase stateBase = new TAGProcessorStateBase(); // Derivatives to construct later
            TAGValueSink sink = new TAGValueSink(stateBase);

            //Read the TAG file
            TAGReadResult result = file.Read(Path.Combine("TestData", "TAGFiles", "TestTAGFile-TAGFile-Read-File.tag"), sink);

            Assert.Equal(TAGReadResult.NoError, result);
        }
    }
}
