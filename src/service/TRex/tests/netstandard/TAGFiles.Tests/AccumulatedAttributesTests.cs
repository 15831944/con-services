﻿using System;
using FluentAssertions;
using VSS.TRex.Common;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.TAGFiles.Classes.ValueMatcher;
using VSS.TRex.Types;
using Xunit;

namespace TAGFiles.Tests
{
        public class AccumulatedAttributesTests
    {
        [Fact]
        public void Test_AccumulatedAttributes_Creation()
        {
            var attrs = new AccumulatedAttributes<byte>();
            Assert.NotNull(attrs);
        }

        [Fact()]
        public void Test_AccumulatedAttributes_DiscardAllButLatest()
        {
            var attrs = new AccumulatedAttributes<int>();

            // Add a couple of attributes, check discard preserves the last one
            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 1), DateTimeKind.Utc), 1);
            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 10), DateTimeKind.Utc), 2);

            attrs.DiscardAllButLatest();

            Assert.Equal(1, attrs.NumAttrs);
            Assert.Equal(2, attrs.GetLatest());
        }

        [Fact()]
        public void Test_AccumulatedAttributes_Count()
        {
            var attrs = new AccumulatedAttributes<int>();

            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 1), DateTimeKind.Utc), 1);
            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 10), DateTimeKind.Utc), 2);

            Assert.Equal(2, attrs.NumAttrs);
        }

        [Fact()]
        public void Test_AccumulatedAttributes_Add()
        {
            var attrs = new AccumulatedAttributes<int>();

            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 1), DateTimeKind.Utc), 1);

            Assert.Equal(1, attrs.NumAttrs);
            Assert.Equal(1, attrs.GetLatest());
        }

        [Fact()]
        public void Test_AccumulatedAttributes_GetLatest()
        {
            var attrs = new AccumulatedAttributes<int>();

            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 1), DateTimeKind.Utc), 1);
            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 2), DateTimeKind.Utc), 2);

            Assert.Equal(2, attrs.GetLatest());
        }

        [Fact()]
        public void Test_AccumulatedAttributes_GetValueAtDateTime_Empty()
        {
          var attrs = new AccumulatedAttributes<int>();

          attrs.GetValueAtDateTime(DateTime.UtcNow, out _).Should().BeFalse();
        }

        [Fact()]
        public void Test_AccumulatedAttributes_GetValueAtDateTime()
        {
            var attrs = new AccumulatedAttributes<int>();

            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 1), DateTimeKind.Utc), 1);
            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 10), DateTimeKind.Utc), 2);

            Assert.True(attrs.GetValueAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 0), DateTimeKind.Utc), out int value) && value== 1, 
                          "Failed to locate first attribute with preceding time");                                                         
                                                                                                                                           
            Assert.True(attrs.GetValueAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 1), DateTimeKind.Utc), out value) && value == 1,
                          "Failed to locate first attribute with exact time");                                                             
                                                                                                                                           
            Assert.True(attrs.GetValueAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 5), DateTimeKind.Utc), out value) &&  value == 1,
                          "Failed to locate first attribute with trailing time");                                                          
                                                                                                                                           
            Assert.True(attrs.GetValueAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 10), DateTimeKind.Utc), out value) && value == 2,
                          "Failed to locate second attribute with exact time");                                                            
                                                                                                                                           
            Assert.True(attrs.GetValueAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 20), DateTimeKind.Utc), out value) && value == 2,
                          "Failed to locate second attribute with trailing time");
        }

        [Fact()]
        public void Test_AccumulatedAttributes_GetGPSModeAtDateTime()
        {
            var attrs = new AccumulatedAttributes<GPSMode>();

            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 1), DateTimeKind.Utc), GPSMode.Fixed);
            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 10), DateTimeKind.Utc), GPSMode.Float);

            Assert.Equal(GPSMode.Fixed, attrs.GetValueAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 5), DateTimeKind.Utc), CellPassConsts.NullGPSMode));

            Assert.Equal(GPSMode.Float, attrs.GetValueAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 15), DateTimeKind.Utc), CellPassConsts.NullGPSMode));
        }

        [Fact()]
        public void Test_AccumulatedAttributes_GetCCVValueAtDateTime()
        {
            var attrs = new AccumulatedAttributes<short>();

            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 1), DateTimeKind.Utc), 10);
            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 10), DateTimeKind.Utc), 20);

            Assert.Equal(10, attrs.GetValueAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 5), DateTimeKind.Utc), CellPassConsts.NullCCV));

            Assert.Equal(20, attrs.GetValueAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 15), DateTimeKind.Utc), CellPassConsts.NullCCV));
        }

        [Fact()]
        public void Test_AccumulatedAttributes_GetRMVValueAtDateTime()
        {
            var attrs = new AccumulatedAttributes<short>();

            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 1), DateTimeKind.Utc), 10);
            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 10), DateTimeKind.Utc), 20);

            Assert.Equal(10, attrs.GetValueAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 5), DateTimeKind.Utc), CellPassConsts.NullRMV));

            Assert.Equal(20, attrs.GetValueAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 15), DateTimeKind.Utc), CellPassConsts.NullRMV));
        }

        [Fact()]
        public void Test_AccumulatedAttributes_GetFrequencyValueAtDateTime()
        {
            var attrs = new AccumulatedAttributes<ushort>();

            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 1), DateTimeKind.Utc), 10);
            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 10), DateTimeKind.Utc), 20);

            Assert.Equal(10, attrs.GetValueAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 5), DateTimeKind.Utc), CellPassConsts.NullFrequency));

            Assert.Equal(20, attrs.GetValueAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 15), DateTimeKind.Utc), CellPassConsts.NullFrequency));
        }

        [Fact()]
        public void Test_AccumulatedAttributes_GetAmplitudeValueAtDateTime()
        {
            var attrs = new AccumulatedAttributes<ushort>();

            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 1), DateTimeKind.Utc), 10);
            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 10), DateTimeKind.Utc), 20);

            Assert.Equal(10, attrs.GetValueAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 5), DateTimeKind.Utc), CellPassConsts.NullAmplitude));

            Assert.Equal(20, attrs.GetValueAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 15), DateTimeKind.Utc), CellPassConsts.NullAmplitude));
        }

        [Fact()]
        public void Test_AccumulatedAttributes_GetAgeOfCorrectionValueAtDateTime()
        {
            var attrs = new AccumulatedAttributes<byte>();

            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 1), DateTimeKind.Utc), 10);
            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 10), DateTimeKind.Utc), 20);

            Assert.Equal(10, attrs.GetValueAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 5), DateTimeKind.Utc), 0));

            Assert.Equal(20, attrs.GetValueAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 15), DateTimeKind.Utc), 0));
        }

        [Fact()]
        public void Test_AccumulatedAttributes_GetOnGroundAtDateTime()
        {
            var attrs = new AccumulatedAttributes<OnGroundState>();

            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 1), DateTimeKind.Utc), OnGroundState.No);
            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 10), DateTimeKind.Utc), OnGroundState.YesLegacy);

            Assert.Equal(OnGroundState.No, attrs.GetValueAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 5), DateTimeKind.Utc), OnGroundState.No));

            Assert.Equal(OnGroundState.YesLegacy, attrs.GetValueAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 15), DateTimeKind.Utc), OnGroundState.No));
        }

        [Fact()]
        public void Test_AccumulatedAttributes_GetMaterialTemperatureValueAtDateTime()
        {
            var attrs = new AccumulatedAttributes<ushort>();

            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 1), DateTimeKind.Utc), 10);
            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 10), DateTimeKind.Utc), 20);

            Assert.Equal(10, attrs.GetValueAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 5), DateTimeKind.Utc), CellPassConsts.NullMaterialTemperatureValue));

            Assert.Equal(20, attrs.GetValueAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 15), DateTimeKind.Utc), CellPassConsts.NullMaterialTemperatureValue));
        }

        [Fact()]
        public void Test_AccumulatedAttributes_GetMDPValueAtDateTime()
        {
            var attrs = new AccumulatedAttributes<short>();

            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 1), DateTimeKind.Utc), 10);
            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 10), DateTimeKind.Utc), 20);

            Assert.Equal(10, attrs.GetValueAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 5), DateTimeKind.Utc), CellPassConsts.NullMDP));

            Assert.Equal(20, attrs.GetValueAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 15), DateTimeKind.Utc), CellPassConsts.NullMDP));
        }

        [Fact()]
        public void Test_AccumulatedAttributes_GetMachineSpeedValueAtDateTime()
        {
            var attrs = new AccumulatedAttributes<double>();

            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 1), DateTimeKind.Utc), 1.0);
            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 10), DateTimeKind.Utc), 2.0);

            Assert.Equal(1.0, attrs.GetValueAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 5), DateTimeKind.Utc), Consts.NullDouble));

            Assert.Equal(2.0, attrs.GetValueAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 15), DateTimeKind.Utc), Consts.NullDouble));
        }

        [Fact()]
        public void Test_AccumulatedAttributes_GetCCAValueAtDateTime()
        {
            var attrs = new AccumulatedAttributes<byte>();

            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 1), DateTimeKind.Utc), 10);
            attrs.Add(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 10), DateTimeKind.Utc), 20);

            Assert.Equal(10, attrs.GetValueAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 5), DateTimeKind.Utc), CellPassConsts.NullCCA));

            Assert.Equal(20, attrs.GetValueAtDateTime(DateTime.SpecifyKind(new DateTime(2000, 1, 1, 1, 1, 15), DateTimeKind.Utc), CellPassConsts.NullCCA));
        }

    }
}
