﻿using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using VSS.VisionLink.Raptor.SubGridTrees;
using VSS.VisionLink.Raptor;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;
using System.Text;
using Xunit;

namespace VSS.VisionLink.Raptor.RaptorClassLibrary.Tests
{
        public class GenericLeafSubGridTests
    {
        [Fact]
        public void Test_GenericLeafSubGridTests_Creation()
        {
            Assert.Fail("Not implemented");
        }

        [Fact]
        public void Test_GenericLeafSubGridTests_ForEach()
        {
            Assert.Fail("Not implemented");
        }

        [Fact]
        public void Test_GenericLeafSubGridTests_Clear()
        {
            Assert.Fail("Not implemented");
        }

        [Fact]
        public void Test_GenericLeafSubGridTests_Read_BinaryReader()
        {
            ISubGridTree tree = new SubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, GenericLeafSubGrid<Double>>());
            GenericLeafSubGrid<Double> subgrid = new GenericLeafSubGrid<double>(tree, null, SubGridTree.SubGridTreeLevels);

            // This is not implemented and should throw an exception. Override to implement...
            try
            {
                subgrid.Read(new BinaryReader(new MemoryStream(), Encoding.UTF8, true), new byte[10000]);
                Assert.Fail("Read with BinaryReader did not throw an exception");
            }
            catch (Exception)
            {
                // As expected
            }
        }

        [Fact]
        public void Test_GenericLeafSubGridTests_Write_BinaryWriter()
        {
            ISubGridTree tree = new SubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, GenericLeafSubGrid<Double>>());
            GenericLeafSubGrid<Double> subgrid = new GenericLeafSubGrid<double>(tree, null, SubGridTree.SubGridTreeLevels);

            // This is not implemented and should throw an exception. Override to implement...
            try
            {
                subgrid.Write(new BinaryWriter(new MemoryStream()), new byte[10000]);
                Assert.Fail("Read with BinaryWrite did not throw an exception");
            }
            catch (Exception)
            {
                // As expected
            }
        }

        [Fact]
        public void Test_GenericLeafSubGridTests_Read_BinaryFormatter()
        {
            Assert.Fail("Not implemented");
        }

        [Fact]
        public void Test_GenericLeafSubGridTests_Write_BinaryFormatter()
        {
            double[,] ary = new double[32, 32];

            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();

            bf.Serialize(ms, ary);

            ms.Position = 0;

            double[,] ary2 = (double[,])bf.Deserialize(ms);

            bool areSame = true;

            for (int i = 0; i < 32*32; i++)
            {
                if (ary[i / 32, i% 32] != ary2[i / 32, i % 32])
                {
                    areSame = false;
                    break;
                }
            }

            Assert.True(areSame, "The two arrays are not the same");
        }

    }
}
