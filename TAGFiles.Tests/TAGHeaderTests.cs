﻿using System;
using VSS.VisionLink.Raptor.TAGFiles.Classes;
using System.IO;
using Xunit;

namespace VSS.VisionLink.Raptor.TAGFiles.Tests
{
        public class TAGHeaderTests
    {
        [Fact]
        public void Test_TAGHeader_Creation()
        {
            TAGHeader header = new TAGHeader();

            Assert.True(header.DictionaryID == 0 &&
                header.DictionaryMajorVer == 0 &&
                header.DictionaryMinorVer == 0 &&
                header.FieldAndTypeTableOffset == 0 &&
                header.MajorVer == 0 &&
                header.MinorVer == 0,
                "Header not created as expected");
        }

        [Fact]
        public void Test_TAGHeader_Read()
        {
            TAGReader reader = new TAGReader(new FileStream(TAGTestConsts.TestDataFilePath() + "TAGFiles\\TestTAGFile-TAG-Header-Read.tag", FileMode.Open));
            //TAGReader reader = new TAGReader(new FileStream(TagTestConsts.TestTAGFileName(), FileMode.Open));

            Assert.NotNull(reader);

            TAGHeader header = new TAGHeader();

            //Read the header
            header.Read(reader);

            Assert.Equal(1, header.DictionaryID);
            Assert.Equal(1, header.DictionaryMajorVer);
            Assert.Equal(4, header.DictionaryMinorVer);
            Assert.Equal(1, header.MajorVer);
            Assert.Equal(0, header.MinorVer);
            Assert.True(header.FieldAndTypeTableOffset > 0 && header.FieldAndTypeTableOffset < reader.GetSize() / 2,
                          "Field and type table offset read from header is invalid");           
        }
    }
}
