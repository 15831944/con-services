﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.MasterData.Repositories.DBModels;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using Xunit;

namespace VSS.MasterData.ProjectTests
{
  public class ProjectGeofenceValidationTestsDiFixture : UnitTestsDIFixture<ProjectGeofenceValidationTestsDiFixture>
  {
    protected ProjectErrorCodesProvider _projectErrorCodesProvider = new ProjectErrorCodesProvider();
    private readonly List<GeofenceWithAssociation> _geofencesWithAssociation;

    public ProjectGeofenceValidationTestsDiFixture()
    {
      var validBoundary =
        "POLYGON((172.595831670724 -43.5427038560109,172.594630041089 -43.5438859356773,172.59329966542 -43.542486101965, 172.595831670724 -43.5427038560109))";
      _geofencesWithAssociation = new List<GeofenceWithAssociation>
                                  {
        new GeofenceWithAssociation
        {
          CustomerUID = Guid.NewGuid().ToString(),
          Name = "geofence Name",
          Description = "geofence Description",
          GeofenceType = GeofenceType.Landfill,
          GeometryWKT = validBoundary,
          FillColor = 4555,
          IsTransparent = false,
          GeofenceUID = Guid.NewGuid().ToString(),
          UserUID = Guid.NewGuid().ToString(),
          AreaSqMeters = 12.45
        },
        new GeofenceWithAssociation
        {
          CustomerUID = Guid.NewGuid().ToString(),
          Name = "geofence Name2",
          Description = "geofence Description2",
          GeofenceType = GeofenceType.Project,
          GeometryWKT = validBoundary,
          FillColor = 4555,
          IsTransparent = false,
          GeofenceUID = Guid.NewGuid().ToString(),
          UserUID = Guid.NewGuid().ToString(),
          AreaSqMeters = 223.45
        },
        new GeofenceWithAssociation
        {
          CustomerUID = Guid.NewGuid().ToString(),
          Name = "geofence Name3",
          Description = "geofence Description3",
          GeofenceType = GeofenceType.Landfill,
          GeometryWKT = validBoundary,
          FillColor = 4555,
          IsTransparent = false,
          GeofenceUID = Guid.NewGuid().ToString(),
          UserUID = Guid.NewGuid().ToString(),
          AreaSqMeters = 43.45,
          ProjectUID = Guid.NewGuid().ToString()
        },
        new GeofenceWithAssociation
        {
          CustomerUID = Guid.NewGuid().ToString(),
          Name = "geofence Name4",
          Description = "geofence Description4",
          GeofenceType = GeofenceType.CutZone,
          GeometryWKT = validBoundary,
          FillColor = 4555,
          IsTransparent = false,
          GeofenceUID = Guid.NewGuid().ToString(),
          UserUID = Guid.NewGuid().ToString(),
          AreaSqMeters = 43.45
        }
      };
    }


    [Fact]
    public void ValidateCopyGeofenceResult()
    {
      var result = new GeofenceV4DescriptorsListResult
      {
        GeofenceDescriptors = _geofencesWithAssociation.Select(geofence =>
            AutoMapperUtility.Automapper.Map<GeofenceV4Descriptor>(geofence))
          .ToImmutableList()
      };

      Assert.Equal(4, result.GeofenceDescriptors.Count);
      Assert.Equal(10, (int) result.GeofenceDescriptors[0].GeofenceType);

      Assert.Equal(_geofencesWithAssociation[1].GeofenceUID, result.GeofenceDescriptors[1].GeofenceUid);
      Assert.Equal(_geofencesWithAssociation[1].Name, result.GeofenceDescriptors[1].Name);
      Assert.Equal(1, (int) result.GeofenceDescriptors[1].GeofenceType);
      Assert.Equal(_geofencesWithAssociation[1].GeometryWKT, result.GeofenceDescriptors[1].GeometryWKT);
      Assert.Equal(_geofencesWithAssociation[1].FillColor, result.GeofenceDescriptors[1].FillColor);
      Assert.Equal(_geofencesWithAssociation[1].IsTransparent, result.GeofenceDescriptors[1].IsTransparent);
      Assert.Equal(_geofencesWithAssociation[1].Description, result.GeofenceDescriptors[1].Description);
      Assert.Equal(_geofencesWithAssociation[1].CustomerUID, result.GeofenceDescriptors[1].CustomerUid);
      Assert.Equal(_geofencesWithAssociation[1].UserUID, result.GeofenceDescriptors[1].UserUid);
      Assert.Equal(_geofencesWithAssociation[1].AreaSqMeters, result.GeofenceDescriptors[1].AreaSqMeters);
    }

    [Fact]
    public void ValidateUpdateProjectGeofenceRequest_HappyPath()
    {
      var projectUid = Guid.NewGuid();
      var geofenceTypes = new List<GeofenceType> {GeofenceType.Landfill};
      var geofences = new List<Guid> {Guid.NewGuid()};
      var request =
        UpdateProjectGeofenceRequest.CreateUpdateProjectGeofenceRequest(projectUid, geofenceTypes, geofences);
      request.Validate();
    }

    [Fact]
    public void ValidateUpdateProjectGeofenceRequest_MissingProjectUid()
    {
      var projectUid = Guid.Empty;
      var geofenceTypes = new List<GeofenceType> {GeofenceType.Landfill};
      var geofences = new List<Guid> {Guid.NewGuid()};
      var request =
        UpdateProjectGeofenceRequest.CreateUpdateProjectGeofenceRequest(projectUid, geofenceTypes, geofences);

      var ex = Assert.Throws<ServiceException>(() => request.Validate());
      Assert.NotEqual(-1,
        ex.GetContent.IndexOf(_projectErrorCodesProvider.FirstNameWithOffset(5), StringComparison.Ordinal));
    }

    [Fact]
    public void ValidateUpdateProjectGeofenceRequest_MissingGeofenceTypes1()
    {
      var projectUid = Guid.NewGuid();
      var geofenceTypes = new List<GeofenceType>();
      var geofences = new List<Guid> {Guid.NewGuid()};
      var request =
        UpdateProjectGeofenceRequest.CreateUpdateProjectGeofenceRequest(projectUid, geofenceTypes, geofences);

      var ex = Assert.Throws<ServiceException>(() => request.Validate());
      Assert.NotEqual(-1,
        ex.GetContent.IndexOf(_projectErrorCodesProvider.FirstNameWithOffset(73), StringComparison.Ordinal));
    }

    [Fact]
    public void ValidateUpdateProjectGeofenceRequest_MissingGeofenceTypes2()
    {
      var projectUid = Guid.NewGuid();
      var geofences = new List<Guid> {Guid.NewGuid()};
      var request =
        UpdateProjectGeofenceRequest.CreateUpdateProjectGeofenceRequest(projectUid, null, geofences);

      var ex = Assert.Throws<ServiceException>(() => request.Validate());
      Assert.NotEqual(-1,
        ex.GetContent.IndexOf(_projectErrorCodesProvider.FirstNameWithOffset(73), StringComparison.Ordinal));
    }

    [Fact]
    public void ValidateUpdateProjectGeofenceRequest_UnsupportedGeofenceTypes()
    {
      var projectUid = Guid.NewGuid();
      var geofenceTypes = new List<GeofenceType> {GeofenceType.CutZone};
      var geofences = new List<Guid> {Guid.NewGuid(), Guid.NewGuid() };
      var request =
        UpdateProjectGeofenceRequest.CreateUpdateProjectGeofenceRequest(projectUid, geofenceTypes, geofences);

      var ex = Assert.Throws<ServiceException>(() => request.Validate());
      Assert.NotEqual(-1,
        ex.GetContent.IndexOf(_projectErrorCodesProvider.FirstNameWithOffset(102), StringComparison.Ordinal));
    }

    [Fact]
    public void ValidateUpdateProjectGeofenceRequest_MissingGeofenceUids1()
    {
      var projectUid = Guid.NewGuid();
      var geofenceTypes = new List<GeofenceType> {GeofenceType.Landfill};
      var request =
        UpdateProjectGeofenceRequest.CreateUpdateProjectGeofenceRequest(projectUid, geofenceTypes, null);

      var ex = Assert.Throws<ServiceException>(() => request.Validate());
      Assert.NotEqual(-1,
        ex.GetContent.IndexOf(_projectErrorCodesProvider.FirstNameWithOffset(103), StringComparison.Ordinal));
    }

    [Fact]
    public void ValidateUpdateProjectGeofenceRequest_MissingGeofenceUids2()
    {
      var projectUid = Guid.NewGuid();
      var geofenceTypes = new List<GeofenceType> {GeofenceType.Landfill};
      var geofences = new List<Guid>();
      var request =
        UpdateProjectGeofenceRequest.CreateUpdateProjectGeofenceRequest(projectUid, geofenceTypes, geofences);

      var ex = Assert.Throws<ServiceException>(() => request.Validate());
      Assert.NotEqual(-1,
        ex.GetContent.IndexOf(_projectErrorCodesProvider.FirstNameWithOffset(103), StringComparison.Ordinal));
    }

    [Fact]
    public void ValidateUpdateProjectGeofenceRequest_MissingGeofenceUids3()
    {
      var projectUid = Guid.NewGuid();
      var geofenceTypes = new List<GeofenceType> {GeofenceType.Landfill};
      var geofences = new List<Guid> {Guid.Empty};
      var request =
        UpdateProjectGeofenceRequest.CreateUpdateProjectGeofenceRequest(projectUid, geofenceTypes, geofences);

      var ex = Assert.Throws<ServiceException>(() => request.Validate());
      Assert.NotEqual(-1,
        ex.GetContent.IndexOf(_projectErrorCodesProvider.FirstNameWithOffset(103), StringComparison.Ordinal));
    }

    [Fact]
    public void ValidateUpdateProjectGeofenceRequest_DuplicateGeofenceUids()
    {
      var projectUid = Guid.NewGuid();
      var geofenceTypes = new List<GeofenceType> { GeofenceType.Landfill };
      var geofenceUid1 = Guid.NewGuid();
      var geofenceUid2 = Guid.NewGuid();
      var geofences = new List<Guid> { geofenceUid1, geofenceUid2, geofenceUid1 };
      var request =
        UpdateProjectGeofenceRequest.CreateUpdateProjectGeofenceRequest(projectUid, geofenceTypes, geofences);

      var ex = Assert.Throws<ServiceException>(() => request.Validate());
      Assert.NotEqual(-1,
        ex.GetContent.IndexOf(_projectErrorCodesProvider.FirstNameWithOffset(110), StringComparison.Ordinal));
    }

    [Fact]
    public void ValidateGeodList()
    {
      var boundaryLL = new List<TBCPoint>
                       {
        new TBCPoint(-43.5, 172.6),
        new TBCPoint(-43.5003, 172.6),
        new TBCPoint(-43.5003, 172.603),
        new TBCPoint(-43.5, 172.603)
      };
      var serialized = JsonConvert.SerializeObject(boundaryLL);
      Assert.Equal(@"[{""Latitude"":-43.5,""Longitude"":172.6},{""Latitude"":-43.5003,""Longitude"":172.6},{""Latitude"":-43.5003,""Longitude"":172.603},{""Latitude"":-43.5,""Longitude"":172.603}]", serialized);
    }
  }
}
