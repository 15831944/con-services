﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Common.Utilities;

namespace VSS.Productivity3D.WebApiTests.RaptorServicesCommon.Models
{
  [TestClass]
  public class FilterTests
  {
        [TestMethod]
    public void CanCreateFilterTest()
    {
      var validator = new DataAnnotationsValidator();
      //everything filter
      List<WGSPoint> latLngs = new List<WGSPoint>
                                {
                                    WGSPoint.CreatePoint(35.13*ConversionConstants.DEGREES_TO_RADIANS, 179.2*ConversionConstants.DEGREES_TO_RADIANS),
                                    WGSPoint.CreatePoint(34.25*ConversionConstants.DEGREES_TO_RADIANS, 178.1*ConversionConstants.DEGREES_TO_RADIANS),
                                    WGSPoint.CreatePoint(36.4*ConversionConstants.DEGREES_TO_RADIANS, 177.34*ConversionConstants.DEGREES_TO_RADIANS)
                                };
      List<Point> gridPoints = new List<Point>
                                {
                                    Point.CreatePoint(12.4, 126.5),
                                    Point.CreatePoint(25.6, 99.2),
                                    Point.CreatePoint(15.2, 45.2),
                                    Point.CreatePoint(21.5, 89.3)
                                };
      DesignDescriptor desc = DesignDescriptor.CreateDesignDescriptor(1111, null, 0);
      List<MachineDetails> machines = new List<MachineDetails>
                                                {
                                                    MachineDetails.CreateMachineDetails(12345678, "Acme Compactor 1", false),
                                                };

      Filter filter = Filter.CreateFilter(null, null, null, new DateTime(2014, 1, 1), new DateTime(2014, 1, 31), 1111, new List<long>{12345678, 87654321}, true,
          true, ElevationType.First, latLngs, gridPoints, 
          true, desc, 5.0, 100.0, 1.0, 2.0, "Acme Compactor", FilterLayerMethod.OffsetFromBench, desc, 0.3, 2,
          0.35, machines, new List<long> {1, 2, 3, 4}, true,GPSAccuracy.Medium, false, true, false, false);
      ICollection<ValidationResult> results;
      Assert.IsTrue(validator.TryValidate(filter, out results));

      //null filter
      filter = Filter.CreateFilter(null, null, null, null, null, null, null, null, null, null, null, null, true, null, null, null,
        null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null);
      Assert.IsTrue(validator.TryValidate(filter, out results), "null filter failed");

      //start station out of range
      filter = Filter.CreateFilter(null, null, null, null, null, null, null, null, null, null, null, null, null, null, -10001, null,
        null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null);
      Assert.IsFalse(validator.TryValidate(filter, out results), "start station validate failed");

      //end station out of range
      filter = Filter.CreateFilter(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 1000005,
        null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null);
      Assert.IsFalse(validator.TryValidate(filter, out results), "end station validate failed");

      //left offset out of range
      filter = Filter.CreateFilter(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
        null, 777, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null);
      Assert.IsFalse(validator.TryValidate(filter, out results), "left offset validate failed");

      //right offset out of range
      filter = Filter.CreateFilter(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
        null, null, 987, null, null, null, null, null, null, null, null, null, null, null, null, null, null);
      Assert.IsFalse(validator.TryValidate(filter, out results), "right offset validate failed");

      //bench elevation out of range
      filter = Filter.CreateFilter(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, 111111, null, null, null, null, null, null, null, null, null, null);
      Assert.IsFalse(validator.TryValidate(filter, out results), "bench elevation validate failed");

      //layer number out of range
      filter = Filter.CreateFilter(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, -9876, null, null, null, null, null, null, null, null, null);
      Assert.IsFalse(validator.TryValidate(filter, out results), "layer number validate failed");

      //layer thickness out of range
      filter = Filter.CreateFilter(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, 0.000001, null, null, null, null, null, null, null, null);
      Assert.IsFalse(validator.TryValidate(filter, out results), "layer thickness validate failed");
          
    }

        [TestMethod]
        public void ValidateSuccessTest()
        {
          DesignDescriptor desc = DesignDescriptor.CreateDesignDescriptor(1, null, 2.0);
          Filter filter = Filter.CreateFilter(null, null, null, new DateTime(2014, 1, 1), new DateTime(2014, 1, 31), null, null, 
            null, null, null, null, null, null, desc, 100.0, 500.0, 1.0, 2.0, null, FilterLayerMethod.OffsetFromDesign, desc,
            null, 2, 0.5, null, null, null, null, null, null, null, null);
          filter.Validate();   
        }

 

    [TestMethod]
    public void ValidateFailInvalidDateRangeTest()
    {
      //start UTC > end UTC
      Filter filter = Filter.CreateFilter(null, null, null, new DateTime(2014, 1, 31), new DateTime(2014, 1, 1), null, 
        null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null);
      Assert.ThrowsException<ServiceException>(() => filter.Validate());
    }

    [TestMethod]
    public void ValidateFailInvalidAlignmentFilterTest()
    {
      //missing alignment filter fields
      Filter filter = Filter.CreateFilter(null, null, null, null, null, null, null, null, null, null, null, null, null,
        null, 100.0, 500.0, 2.0, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null);
      Assert.ThrowsException<ServiceException>(() => filter.Validate());
    }

    [TestMethod]
    public void ValidateFailInvalidLayerFilterTest()
    {
      //Invalid layer filter
      Filter filter = Filter.CreateFilter(null, null, null, null, null, null, null, null, null, null, null, null, null, 
        null, null, null, null, null, null, FilterLayerMethod.OffsetFromBench, null, null, null, null, null, null, null,
        null, null, null, null, null);
      Assert.ThrowsException<ServiceException>(() => filter.Validate());
    }

    [TestMethod]
    public void ValidateFailInvalidLatLngPointsTest()
    {
      //too few points
      Filter filter = Filter.CreateFilter(null, null, null, null, null, null, null, null, null, null, new List<WGSPoint>(),
        null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null);
      Assert.ThrowsException<ServiceException>(() => filter.Validate());
    }

    [TestMethod]
    public void ValidateFailInvalidGridPointsTest()
    {
      //too few points
      Filter filter = Filter.CreateFilter(null, null, null, null, null, null, null, null, null, null, null, new List<Point>(),
        null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null);
      Assert.ThrowsException<ServiceException>(() => filter.Validate());

    }

    [TestMethod]
    public void ValidateFailInvalidBoundaryFilterTest()
    {
      //both LL and grid points specified
      List<WGSPoint> latLngs = new List<WGSPoint>
                            {
                                WGSPoint.CreatePoint(35.13*ConversionConstants.DEGREES_TO_RADIANS, 179.2*ConversionConstants.DEGREES_TO_RADIANS),
                                WGSPoint.CreatePoint(34.25*ConversionConstants.DEGREES_TO_RADIANS, 178.1*ConversionConstants.DEGREES_TO_RADIANS),
                                WGSPoint.CreatePoint(36.4*ConversionConstants.DEGREES_TO_RADIANS, 177.34*ConversionConstants.DEGREES_TO_RADIANS)
                            };
      List<Point> gridPoints = new List<Point>
                            {
                                Point.CreatePoint(12.4, 126.5),
                                Point.CreatePoint(25.6, 99.2),
                                Point.CreatePoint(15.2, 45.2),
                                Point.CreatePoint(21.5, 89.3)
                            };
      Filter filter = Filter.CreateFilter(null, null, null, null, null, null, null, null, null, null, latLngs, gridPoints,
        null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null);
      Assert.ThrowsException<ServiceException>(() => filter.Validate());

    }


  }
}
