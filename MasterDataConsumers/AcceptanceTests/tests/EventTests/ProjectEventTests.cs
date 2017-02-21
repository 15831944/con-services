﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestUtility;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace EventTests
{
  [TestClass]
  public class ProjectEventTests
  {
//    private const string PROJECT_DB_SCHEMA_NAME = "VSS-MasterData-Project-Only";

    [TestMethod]
    public void CreateProjectEvent()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
  //    mysql.updateDBSchemaName(PROJECT_DB_SCHEMA_NAME);
      var projectGuid = Guid.NewGuid();
      DateTime startDate = testSupport.ConvertVSSDateString("-1d+00:00:00");
      DateTime endDate = testSupport.ConvertVSSDateString("2d+00:00:00");
      msg.Title("Create Project test 1", "Create one project");
      var eventArray = new[] {
             "| EventType          | EventDate   | ProjectID | ProjectUID    | ProjectName   | ProjectType                     | ProjectTimezone           | ProjectStartDate | ProjectEndDate | ProjectBoundary |" ,
            $"| CreateProjectEvent | 0d+09:00:00 | 1         | {projectGuid} | testProject1  | {ProjectType.ProjectMonitoring} | New Zealand Standard Time | {startDate}      | {endDate}      | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |"};

      testSupport.InjectEventsIntoKafka(eventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Project", "ProjectUID",
        "Name, LegacyProjectID, fk_ProjectTypeID, StartDate, EndDate", //Fields
        $"testProject1, 1, {(int)ProjectType.ProjectMonitoring}, {startDate}, {endDate}", //Expected
        projectGuid);
    }

    [TestMethod]
    public void CreateInvalidProjectEvent_StartAfterEnd()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
//      mysql.updateDBSchemaName(PROJECT_DB_SCHEMA_NAME);
      var projectGuid = Guid.NewGuid();
      DateTime startDate = testSupport.ConvertVSSDateString("0d+00:00:00");
      DateTime endDate = testSupport.ConvertVSSDateString("-2d+00:00:00");
      msg.Title("Create Project test 1", "Create one project");
      var eventArray = new[] {
             "| EventType          | EventDate   | ProjectID | ProjectUID      | ProjectName   | ProjectType                     | ProjectTimezone           | ProjectStartDate | ProjectEndDate | ProjectBoundary |" ,
            $"| CreateProjectEvent | 0d+09:00:00 | 1         | { projectGuid } | testProject2  | {ProjectType.ProjectMonitoring} | New Zealand Standard Time | {startDate}      | {endDate}      | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |"};

      testSupport.InjectEventsIntoKafka(eventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 0, projectGuid); //no records should be inserted
      //mysql.VerifyTestResultDatabaseFieldsAreExpected("Project", "ProjectUID",
      //  "Name, LegacyProjectID, fk_ProjectTypeID, StartDate, EndDate", //Fields
      //  $"testProject1, 1, {(int)ProjectType.ProjectMonitoring}, {startDate}, {endDate}", //Expected
      //  projectGuid);
    }

    [TestMethod]
    public void CreateProjectWithSameGuidAgain()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
//      mysql.updateDBSchemaName(PROJECT_DB_SCHEMA_NAME);
      var projectGuid = Guid.NewGuid();
      DateTime startDate = testSupport.ConvertVSSDateString("0d+00:00:00");
      DateTime endDate = testSupport.ConvertVSSDateString("900d+00:00:00");
      msg.Title("Create Project test 1", "Create one project");
      var eventArray = new[] {
         "| EventType          | EventDate   | ProjectID | ProjectUID      | ProjectName   | ProjectType                     | ProjectTimezone           | ProjectStartDate | ProjectEndDate | ProjectBoundary |" ,
        $"| CreateProjectEvent | 0d+09:00:00 | 1         | { projectGuid } | testProject3  | {ProjectType.ProjectMonitoring} | New Zealand Standard Time | {startDate}      | {endDate}      | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |",
        $"| CreateProjectEvent | 0d+09:00:00 | 1         | { projectGuid } | testProject4  | {ProjectType.ProjectMonitoring} | New Zealand Standard Time | {startDate}      | {endDate}      | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |"};

      testSupport.InjectEventsIntoKafka(eventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Project", "ProjectUID",
        "Name, LegacyProjectID, fk_ProjectTypeID, StartDate, EndDate", //Fields
        $"testProject3, 1, {(int)ProjectType.ProjectMonitoring}, {startDate}, {endDate}", //Expected
        projectGuid);
    }

    [TestMethod]
    public void CreateStandardProjectWithProjectType()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
//      mysql.updateDBSchemaName(PROJECT_DB_SCHEMA_NAME);
      var projectGuid = Guid.NewGuid();
      DateTime startDate = testSupport.ConvertVSSDateString("0d+00:00:00");
      DateTime endDate = testSupport.ConvertVSSDateString("10000d+00:00:00");
      msg.Title("Create Project test 1", "Create one project");
      var eventArray = new[] {
        "| EventType          | EventDate   | ProjectID | ProjectUID      | ProjectName     | ProjectType            | ProjectTimezone            | ProjectStartDate | ProjectEndDate | ProjectBoundary |" ,
       $"| CreateProjectEvent | 0d+09:00:00 | 1         | { projectGuid } | testProject5    | {ProjectType.Standard} | New Zealand Standard Time  | {startDate}      | {endDate}      | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |"  };

      testSupport.InjectEventsIntoKafka(eventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Project", "ProjectUID",
        "Name, LegacyProjectID, fk_ProjectTypeID, StartDate, EndDate", //Fields
        $"testProject5, 1, {(int)ProjectType.Standard}, {startDate}, {endDate}", //Expected
        projectGuid);
    }



    [TestMethod]
    public void UpdateProject_Change_ProjectType()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
//      mysql.updateDBSchemaName(PROJECT_DB_SCHEMA_NAME);
      var projectGuid = Guid.NewGuid();
      string projectName = "testProject8";
      DateTime startDate = testSupport.ConvertVSSDateString("0d+00:00:00");
      DateTime endDate = testSupport.ConvertVSSDateString("10000d+00:00:00");
      msg.Title("Create Project test 1", "Create one project");
      var eventArray = new[] {
         "| EventType          | EventDate   | ProjectID | ProjectUID    | ProjectName   | ProjectType            | ProjectTimezone           | ProjectStartDate | ProjectEndDate | ProjectBoundary |" ,
        $"| CreateProjectEvent | 0d+09:00:00 | 1         | {projectGuid} | {projectName} | {ProjectType.LandFill} | New Zealand Standard Time | {startDate}      | {endDate}      | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |",
        $"| UpdateProjectEvent | 0d+09:01:00 | 1         | {projectGuid} | {projectName} | {ProjectType.Standard} | New Zealand Standard Time | {startDate}      | {endDate}      | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |"};

      testSupport.InjectEventsIntoKafka(eventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Project", "ProjectUID",
        "Name, LegacyProjectID, fk_ProjectTypeID, StartDate, EndDate", //Fields
        $"{projectName}, 1, {(int)ProjectType.Standard}, {startDate}, {endDate}", //Expected
        projectGuid);
    }


    [TestMethod]
    public void UpdateProject_Change_ProjectEndDate()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
  //    mysql.updateDBSchemaName(PROJECT_DB_SCHEMA_NAME);
      var projectGuid = Guid.NewGuid();
      string projectName = "testProject10";
      DateTime startDate = testSupport.ConvertVSSDateString("0d+00:00:00");
      DateTime endDate = testSupport.ConvertVSSDateString("42d+00:00:00");
      msg.Title("Create Project test 1", "Create one project");
      var eventArray = new[] {
         "| EventType          | EventDate   | ProjectID | ProjectUID      | ProjectName   | ProjectType            | ProjectTimezone           | ProjectStartDate | ProjectEndDate | ProjectBoundary |" ,
        $"| CreateProjectEvent | 0d+09:00:00 | 1         | { projectGuid } | {projectName} | {ProjectType.LandFill} | New Zealand Standard Time | {startDate}      | {endDate}      | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |",
        $"| UpdateProjectEvent | 0d+09:00:00 | 1         | { projectGuid } | {projectName} | {ProjectType.LandFill} | New Zealand Standard Time | {startDate}      | {endDate.AddYears(10)}  | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |"};

      testSupport.InjectEventsIntoKafka(eventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Project", "ProjectUID",
        "Name, LegacyProjectID, fk_ProjectTypeID, StartDate, EndDate", //Fields
        $"{projectName}, 1, {(int)ProjectType.LandFill}, {startDate}, {endDate.AddYears(10)}", //Expected
        projectGuid);
    }

    [TestMethod]
    public void UpdateProject_Change_ProjectEndDateBeforeStartDate()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
//      mysql.updateDBSchemaName(PROJECT_DB_SCHEMA_NAME);
      var projectGuid = Guid.NewGuid();
      string projectName = "testProject11";
      DateTime startDate = testSupport.ConvertVSSDateString("0d+00:00:00");
      DateTime endDate = testSupport.ConvertVSSDateString("42d+00:00:00");
      msg.Title("Create Project test 10", "Create one project");
      var eventArray = new[] {
        " | EventType          | EventDate   | ProjectID | ProjectUID      | ProjectName   | ProjectType            | ProjectTimezone           | ProjectStartDate | ProjectEndDate | ProjectBoundary |" ,
        $"| CreateProjectEvent | 0d+09:00:00 | 1         | { projectGuid } | {projectName} | {ProjectType.LandFill} | New Zealand Standard Time | {startDate}      | {endDate}      | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |",
        $"| UpdateProjectEvent | 0d+09:00:00 | 1         | { projectGuid } | {projectName} | {ProjectType.LandFill} | New Zealand Standard Time | {startDate}      | {startDate.AddDays(-1)}  | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |"};

      testSupport.InjectEventsIntoKafka(eventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Project", "ProjectUID",
        "Name, LegacyProjectID, fk_ProjectTypeID, StartDate, EndDate", //Fields
        $"{projectName}, 1, {(int)ProjectType.LandFill}, {startDate}, {endDate}", //Expected
        projectGuid);
    }



    [TestMethod]
    public void UpdateProject_Change_ProjectName()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
  //    mysql.updateDBSchemaName(PROJECT_DB_SCHEMA_NAME);
      var projectGuid = Guid.NewGuid();
      string projectName = $"Test Project 12";
      DateTime startDate = testSupport.ConvertVSSDateString("0d+00:00:00");
      DateTime endDate = testSupport.ConvertVSSDateString("10000d+00:00:00");
      msg.Title("Create Project test 1", "Create one project");
      var eventArray = new[] {
        " | EventType          | EventDate   | ProjectID | ProjectUID    | ProjectName   | ProjectType            | ProjectTimezone           | ProjectStartDate | ProjectEndDate | ProjectBoundary |" ,
        $"| CreateProjectEvent | 0d+09:00:00 | 1         | {projectGuid} | testProject11 | {ProjectType.LandFill} | New Zealand Standard Time | {startDate}      | {endDate}      | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |",
        $"| UpdateProjectEvent | 0d+09:00:00 | 1         | {projectGuid} | {projectName} | {ProjectType.LandFill} | New Zealand Standard Time | {startDate}      | {endDate}      | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |"};

      testSupport.InjectEventsIntoKafka(eventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Project", "ProjectUID",
        "Name, LegacyProjectID, fk_ProjectTypeID, StartDate, EndDate", //Fields
        $"{projectName}, 1, {(int)ProjectType.LandFill}, {startDate}, {endDate}", //Expected
        projectGuid);
    }


    [TestMethod]
    public void Create_Then_Delete_Project()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
//      mysql.updateDBSchemaName(PROJECT_DB_SCHEMA_NAME);
      var projectGuid = Guid.NewGuid();
      string projectName = $"Test Project 13";
      DateTime startDate = testSupport.ConvertVSSDateString("0d+00:00:00");
      DateTime endDate = testSupport.ConvertVSSDateString("10000d+00:00:00");
      msg.Title("Create Project test 13", "Create one project, then delete it");
      var eventArray = new[] {
         "| EventType          | EventDate   | ProjectID | ProjectUID      | ProjectName    | ProjectType            | ProjectTimezone           | ProjectStartDate | ProjectEndDate | ProjectBoundary |" ,
        $"| CreateProjectEvent | 0d+09:00:00 | 1         | { projectGuid } | {projectName}  | {ProjectType.LandFill} | New Zealand Standard Time | {startDate}     | {endDate}      | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |",
        $"| DeleteProjectEvent | 0d+09:00:00 | 1         | { projectGuid } | {projectName}  | {ProjectType.LandFill} | New Zealand Standard Time | {startDate}     | {endDate}      | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |"};

      testSupport.InjectEventsIntoKafka(eventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Project", "ProjectUID",
        "Name, LegacyProjectID, fk_ProjectTypeID, IsDeleted, StartDate, EndDate", //Fields
        $"{projectName}, 1, {(int)ProjectType.LandFill}, 1, {startDate}, {endDate}", //Expected
        projectGuid);
    }


    [TestMethod]
    public void Associate_Customer_With_Project()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
  //    mysql.updateDBSchemaName(PROJECT_DB_SCHEMA_NAME);
      var projectGuid = Guid.NewGuid();
      var customerGuid = Guid.NewGuid();
      string projectName = $"Test Project 14";
      DateTime startDate = testSupport.ConvertVSSDateString("0d+00:00:00");
      DateTime endDate = testSupport.ConvertVSSDateString("10000d+00:00:00");


      var customerEventArray = new[] {
             "| EventType           | EventDate   | CustomerName | CustomerType | CustomerUID   |",
            $"| CreateCustomerEvent | 0d+09:00:00 | CustName     | Customer     | {customerGuid} |"};

      testSupport.InjectEventsIntoKafka(customerEventArray); //Create customer to associate project with

      msg.Title("Create Project test 14", "Create one project");
      var projectEventArray = new[] {
        "| EventType          | EventDate   | ProjectID | ProjectUID      | ProjectName   | ProjectType            | ProjectTimezone           | ProjectStartDate | ProjectEndDate | ProjectBoundary |" ,
       $"| CreateProjectEvent | 0d+09:00:00 | 1         | { projectGuid } | {projectName} | {ProjectType.LandFill} | New Zealand Standard Time | {startDate}      | {endDate}      | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |"};

      testSupport.InjectEventsIntoKafka(projectEventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid);


      var associateEventArray = new[] {
        "| EventType                | EventDate   | ProjectUID    | CustomerUID    | ",
       $"| AssociateProjectCustomer | 0d+09:00:00 | {projectGuid} | {customerGuid} | "};


      testSupport.InjectEventsIntoKafka(associateEventArray);
      //Verify project has been associated
      mysql.VerifyTestResultDatabaseFieldsAreExpected("CustomerProject", "fk_ProjectUID",
        "fk_CustomerUID, fk_ProjectUID", //Fields
        $"{customerGuid}, {projectGuid}", //Expected
        projectGuid);
    }


    [TestMethod]
    public void Associate_Geofence_With_Project()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
      var projectMysql = new MySqlHelper();
//      projectMysql.updateDBSchemaName(PROJECT_DB_SCHEMA_NAME);
      var customerGuid = Guid.NewGuid();
      var projectGuid = Guid.NewGuid();
      var geofenceGuid = Guid.NewGuid();
      var userGuid = Guid.NewGuid();
      string projectName = $"Test Project 15";
      DateTime startDate = testSupport.ConvertVSSDateString("0d+00:00:00");
      DateTime endDate = testSupport.ConvertVSSDateString("10000d+00:00:00");


      var geofenceEventArray = new[] {
         "| EventType           | EventDate   | CustomerUID    | Description | FillColor | GeofenceName | GeofenceType | GeofenceUID    | GeometryWKT | IsTransparent | UserUID    | ",
        $"| CreateGeofenceEvent | 0d+09:00:00 | {customerGuid} | Fence       | 1         | SuperFence   | 0            | {geofenceGuid} | 1,2,3,4,5,6 | {false}       | {userGuid} |"};

      testSupport.InjectEventsIntoKafka(geofenceEventArray); //Create customer to associate project with
      mysql.VerifyTestResultDatabaseRecordCount("Geofence", "GeofenceUID", 1, geofenceGuid);

      msg.Title("Create Project test 15", "Create one project");
      var projectEventArray = new[] {
        "| EventType          | EventDate   | ProjectID | ProjectUID    | ProjectName   | ProjectType            | ProjectTimezone           | ProjectStartDate | ProjectEndDate | ProjectBoundary |" ,
       $"| CreateProjectEvent | 0d+09:00:00 | 1         | {projectGuid} | {projectName} | {ProjectType.LandFill} | New Zealand Standard Time | {startDate}      | {endDate}      |POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) | "};

      testSupport.InjectEventsIntoKafka(projectEventArray);
      projectMysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid);

      var associateEventArray = new[] {
        "| EventType                | EventDate   | ProjectUID    | GeofenceUID    | ",
       $"| AssociateProjectGeofence | 0d+09:00:00 | {projectGuid} | {geofenceGuid} | "};
      
      testSupport.InjectEventsIntoKafka(associateEventArray);

      projectMysql.VerifyTestResultDatabaseRecordCount("ProjectGeofence", "fk_GeofenceUID", 1, geofenceGuid);
      projectMysql.VerifyTestResultDatabaseFieldsAreExpected("ProjectGeofence", "fk_GeofenceUID",
        "fk_GeofenceUID, fk_ProjectUID", //Fields
        $"{geofenceGuid}, {projectGuid}", //Expected
        geofenceGuid);
    }


  }
}
