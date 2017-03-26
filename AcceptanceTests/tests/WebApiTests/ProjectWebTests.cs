﻿using System;
using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using TestUtility;
using WebApiModels.ResultHandling;
using WebApiModels.Models;

namespace WebApiTests
{
  [TestClass]
  public class ProjectWebTests
  {
    private readonly Msg msg = new Msg();

    [TestMethod]
    public void NoSubscriptionAssetanddevicecallwebApIwithprojectid()
    {
      msg.Title("Project WebTest 1", "No subscription for standard project. Get the project id for valid tag file request");
      var ts = new TestSupport { IsPublishToKafka = false };
      var legacyProjectId = ts.SetLegacyProjectId();
      var legacyAssetId = ts.SetLegacyAssetId();
      var tccOrg = Guid.NewGuid();
      var deviceUid = Guid.NewGuid();
      // Write events to database 
      var assetEventArray = new[] {
       "| TableName | EventDate   | AssetUID      | LegacyAssetID   | Name            | MakeCode | SerialNumber | Model | IconKey | AssetType  | OwningCustomerUID |",
      $"| Asset     | 0d+09:00:00 | {ts.AssetUid} | {legacyAssetId} | ProjectWebTest1 | CAT      | XAT1         | 345D  | 10      | Excavators |                   |"};
      ts.PublishEventCollection(assetEventArray);
      var deviceEventArray = new[] {
       "| TableName         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | fk_AssetUID   | fk_DeviceUID | fk_SubscriptionUID | EffectiveDate | ",
      $"| Device            | 0d+09:00:00 | {deviceUid}        | Subscribed  | Series522  | {deviceUid} | CDMA         | ProjectWebTest1           |               |              |                    |               |",
      $"| AssetDevice       | 0d+09:20:00 |                    |             |            |             |              |                           | {ts.AssetUid} | {deviceUid}  |                    |               |"};
      ts.PublishEventCollection(deviceEventArray);

      var actualResult = CallWebApiGetProjectId(ts, legacyAssetId, 38.837, -121.348, ts.FirstEventDate.AddDays(1), tccOrg.ToString());
      Assert.AreEqual(-1, actualResult.projectId, " Legacy project's do not match");
      Assert.AreEqual(false, actualResult.result, " result of request doesn't match expected");
    }

    [TestMethod]
    public void ThreeDpmSubscriptionAssetanddevicecallwebApIwithprojectid()
    {
      msg.Title("Project WebTest 2", "3D project monitoring subscription for standard project. Get the project id for valid tag file request");
      var ts = new TestSupport { IsPublishToKafka = false };
      var legacyProjectId = ts.SetLegacyProjectId();
      var legacyAssetId = ts.SetLegacyAssetId();
      var projectUid = Guid.NewGuid();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var deviceUid = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDate = ts.FirstEventDate.ToString("yyyy-MM-dd");
      var endDate = new DateTime(9999, 12, 31).ToString("yyyy-MM-dd");
      var geometryWKT = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      // Write events to database 
      var projectEventArray = new[] {
       "| TableName | EventDate   | ProjectUID   | LegacyProjectID   | Name            | fk_ProjectTypeID | ProjectTimeZone           | LandfillTimeZone | StartDate   | EndDate   | GeometryWKT   |",
      $"| Project   | 0d+09:00:00 | {projectUid} | {legacyProjectId} | ProjectWebTest1 | 0                | New Zealand Standard Time | Pacific/Auckland | {startDate} | {endDate} | {geometryWKT} |" };
      ts.PublishEventCollection(projectEventArray);
      var eventsArray = new[] {
        "| TableName       | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID |",
       $"| Customer        | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |",
       $"| CustomerTccOrg  | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |",
       $"| Subscription    | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 13               | {startDate} | {endDate}      |               |          |",
       $"| CustomerProject | 0d+09:20:00 |               |           |                   |                   | {customerUid}  |                  |             |                | {projectUid}  |          |"};
      ts.PublishEventCollection(eventsArray);
      var assetEventArray = new[] {
       "| TableName | EventDate   | AssetUID      | LegacyAssetID   | Name            | MakeCode | SerialNumber | Model | IconKey | AssetType  | OwningCustomerUID |",
      $"| Asset     | 0d+09:00:00 | {ts.AssetUid} | {legacyAssetId} | ProjectWebTest1 | CAT      | XAT1         | 345D  | 10      | Excavators | {customerUid}     |"};
      ts.PublishEventCollection(assetEventArray);
      var deviceEventArray = new[] {
       "| TableName         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | fk_AssetUID   | fk_DeviceUID | fk_SubscriptionUID | EffectiveDate | ",
      $"| Device            | 0d+09:00:00 | {deviceUid}        | Subscribed  | Series522  | {deviceUid} | CDMA         | ProjectWebTest2           |               |              |                    |               |",
      $"| AssetDevice       | 0d+09:20:00 |                    |             |            |             |              |                           | {ts.AssetUid} | {deviceUid}  |                    |               |",
      $"| AssetSubscription | 0d+09:20:00 |                    |             |            |             |              |                           | {ts.AssetUid} |              | {subscriptionUid}  | {startDate}   |"};
      ts.PublishEventCollection(deviceEventArray);

      var actualResult = CallWebApiGetProjectId(ts, legacyAssetId, 38.837, -121.348, ts.FirstEventDate.AddDays(1), tccOrg.ToString());
      Assert.AreEqual(legacyProjectId, actualResult.projectId, " Legacy project id's do not match");
      Assert.AreEqual(true, actualResult.result, " result of request doesn't match expected");
    }

    [TestMethod]
    public void ThreeDpmSubscriptionAssetanddevicecallwebApIwithLatLongOutSideBoundary ()
    {
      msg.Title("Project WebTest 3", "3D project monitoring subscription for standard project. Call web api with lat long outside boundary");
      var ts = new TestSupport { IsPublishToKafka = false };
      var legacyProjectId = ts.SetLegacyProjectId();
      var legacyAssetId = ts.SetLegacyAssetId();
      var projectUid = Guid.NewGuid();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var deviceUid = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDate = ts.FirstEventDate.ToString("yyyy-MM-dd");
      var endDate = new DateTime(9999, 12, 31).ToString("yyyy-MM-dd");
      var geometryWKT = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      // Write events to database 
      var projectEventArray = new[] {
       "| TableName | EventDate   | ProjectUID   | LegacyProjectID   | Name            | fk_ProjectTypeID | ProjectTimeZone           | LandfillTimeZone | StartDate   | EndDate   | GeometryWKT   |",
      $"| Project   | 0d+09:00:00 | {projectUid} | {legacyProjectId} | ProjectWebTest1 | 0                | New Zealand Standard Time | Pacific/Auckland | {startDate} | {endDate} | {geometryWKT} |" };
      ts.PublishEventCollection(projectEventArray);
      var eventsArray = new[] {
        "| TableName       | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID |",
       $"| Customer        | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |",
       $"| CustomerTccOrg  | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |",
       $"| Subscription    | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 13               | {startDate} | {endDate}      |               |          |",
       $"| CustomerProject | 0d+09:20:00 |               |           |                   |                   | {customerUid}  |                  |             |                | {projectUid}  |          |"};
      ts.PublishEventCollection(eventsArray);
      var assetEventArray = new[] {
       "| TableName | EventDate   | AssetUID      | LegacyAssetID   | Name            | MakeCode | SerialNumber | Model | IconKey | AssetType  | OwningCustomerUID |",
      $"| Asset     | 0d+09:00:00 | {ts.AssetUid} | {legacyAssetId} | ProjectWebTest1 | CAT      | XAT1         | 345D  | 10      | Excavators | {customerUid}     |"};
      ts.PublishEventCollection(assetEventArray);
      var deviceEventArray = new[] {
       "| TableName         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | fk_AssetUID   | fk_DeviceUID | fk_SubscriptionUID | EffectiveDate | ",
      $"| Device            | 0d+09:00:00 | {deviceUid}        | Subscribed  | Series522  | {deviceUid} | CDMA         | ProjectWebTest3           |               |              |                    |               |",
      $"| AssetDevice       | 0d+09:20:00 |                    |             |            |             |              |                           | {ts.AssetUid} | {deviceUid}  |                    |               |",
      $"| AssetSubscription | 0d+09:20:00 |                    |             |            |             |              |                           | {ts.AssetUid} |              | {subscriptionUid}  | {startDate}   |"};
      ts.PublishEventCollection(deviceEventArray);

      var actualResult = CallWebApiGetProjectId(ts, legacyAssetId, 40.837, -121.348, ts.FirstEventDate.AddDays(1), tccOrg.ToString());
      Assert.AreEqual(-1, actualResult.projectId, " Legacy project id's do not match");
      Assert.AreEqual(false, actualResult.result, " result of request doesn't match expected");
    }


    [TestMethod]
    public void ProjectMonitoringSubscriptioAndProjectCallwebApIwithprojectid()
    {
      msg.Title("Project WebTest 4", "Project monitoring subscription and project. Get the project id for valid tag file request");
      var ts = new TestSupport { IsPublishToKafka = false };
      var legacyProjectId = ts.SetLegacyProjectId();
      var legacyAssetId = ts.SetLegacyAssetId();
      var projectUid = Guid.NewGuid();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var deviceUid = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDate = ts.FirstEventDate.ToString("yyyy-MM-dd");
      var endDate = new DateTime(9999, 12, 31).ToString("yyyy-MM-dd");
      var geometryWKT = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      // Write events to database 
      var projectEventArray = new[] {
       "| TableName | EventDate   | ProjectUID   | LegacyProjectID   | Name            | fk_ProjectTypeID | ProjectTimeZone           | LandfillTimeZone | StartDate   | EndDate   | GeometryWKT   |",
      $"| Project   | 0d+09:00:00 | {projectUid} | {legacyProjectId} | ProjectWebTest1 | 2                | New Zealand Standard Time | Pacific/Auckland | {startDate} | {endDate} | {geometryWKT} |" };
      ts.PublishEventCollection(projectEventArray);
      var eventsArray = new[] {
         "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
        $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |",
        $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
        $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 20               | {startDate} | {endDate}      |               |          |                    |",
        $"| CustomerProject     | 0d+09:20:00 |               |           |                   |                   | {customerUid}  |                  |             |                | {projectUid}  |          |                    |",
        $"| ProjectSubscription | 0d+09:20:00 |               |           |                   |                   |                |                  | {startDate} |                | {projectUid}  |          | {subscriptionUid}  |"};
      ts.PublishEventCollection(eventsArray);
      var assetEventArray = new[] {
       "| TableName | EventDate   | AssetUID      | LegacyAssetID   | Name            | MakeCode | SerialNumber | Model | IconKey | AssetType  | OwningCustomerUID |",
      $"| Asset     | 0d+09:00:00 | {ts.AssetUid} | {legacyAssetId} | ProjectWebTest1 | CAT      | XAT1         | 345D  | 10      | Excavators | {customerUid}     |"};
      ts.PublishEventCollection(assetEventArray);
      var deviceEventArray = new[] {
       "| TableName         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | fk_AssetUID   | fk_DeviceUID | fk_SubscriptionUID | EffectiveDate | ",
      $"| Device            | 0d+09:00:00 | {deviceUid}        | Subscribed  | Series522  | {deviceUid} | CDMA         | ProjectWebTest4           |               |              |                    |               |",
      $"| AssetDevice       | 0d+09:20:00 |                    |             |            |             |              |                           | {ts.AssetUid} | {deviceUid}  |                    |               |",
      $"| AssetSubscription | 0d+09:20:00 |                    |             |            |             |              |                           | {ts.AssetUid} |              | {subscriptionUid}  | {startDate}   |"};
      ts.PublishEventCollection(deviceEventArray);

      var actualResult = CallWebApiGetProjectId(ts, legacyAssetId, 38.837, -121.348, ts.FirstEventDate.AddDays(1), tccOrg.ToString());
      Assert.AreEqual(legacyProjectId, actualResult.projectId, " Legacy project id's do not match");
      Assert.AreEqual(true, actualResult.result, " result of request doesn't match expected");
    }

    [TestMethod]
    public void LandfillSubscriptioAndProjectCallwebApIwithprojectid()
    {
      msg.Title("Project WebTest 5", "Landfill subscription and project. Get the project id for valid tag file request");
      var ts = new TestSupport { IsPublishToKafka = false };
      var legacyProjectId = ts.SetLegacyProjectId();
      var legacyAssetId = ts.SetLegacyAssetId();
      var projectUid = Guid.NewGuid();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var deviceUid = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDate = ts.FirstEventDate.ToString("yyyy-MM-dd");
      var endDate = new DateTime(9999, 12, 31).ToString("yyyy-MM-dd");
      var geometryWKT = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      // Write events to database 
      var projectEventArray = new[] {
       "| TableName | EventDate   | ProjectUID   | LegacyProjectID   | Name            | fk_ProjectTypeID | ProjectTimeZone           | LandfillTimeZone | StartDate   | EndDate   | GeometryWKT   |",
      $"| Project   | 0d+09:00:00 | {projectUid} | {legacyProjectId} | ProjectWebTest5 | 1                | New Zealand Standard Time | Pacific/Auckland | {startDate} | {endDate} | {geometryWKT} |" };
      ts.PublishEventCollection(projectEventArray);
      var eventsArray = new[] {
         "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
        $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |",
        $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
        $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                    |",
        $"| CustomerProject     | 0d+09:20:00 |               |           |                   |                   | {customerUid}  |                  |             |                | {projectUid}  |          |                    |",
        $"| ProjectSubscription | 0d+09:20:00 |               |           |                   |                   |                |                  | {startDate} |                | {projectUid}  |          | {subscriptionUid}  |"};
      ts.PublishEventCollection(eventsArray);
      var assetEventArray = new[] {
       "| TableName | EventDate   | AssetUID      | LegacyAssetID   | Name            | MakeCode | SerialNumber | Model | IconKey | AssetType  | OwningCustomerUID |",
      $"| Asset     | 0d+09:00:00 | {ts.AssetUid} | {legacyAssetId} | ProjectWebTest5 | CAT      | XAT1         | 345D  | 10      | Excavators | {customerUid}     |"};
      ts.PublishEventCollection(assetEventArray);
      var deviceEventArray = new[] {
       "| TableName         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | fk_AssetUID   | fk_DeviceUID | fk_SubscriptionUID | EffectiveDate | ",
      $"| Device            | 0d+09:00:00 | {deviceUid}        | Subscribed  | Series522  | {deviceUid} | CDMA         | ProjectWebTest5           |               |              |                    |               |",
      $"| AssetDevice       | 0d+09:20:00 |                    |             |            |             |              |                           | {ts.AssetUid} | {deviceUid}  |                    |               |",
      };
      ts.PublishEventCollection(deviceEventArray);

      var actualResult = CallWebApiGetProjectId(ts, legacyAssetId, 38.8361908, -121.347190, ts.FirstEventDate.AddDays(1), tccOrg.ToString());
      Assert.AreEqual(legacyProjectId, actualResult.projectId, " Legacy project id's do not match");
      Assert.AreEqual(true, actualResult.result, " result of request doesn't match expected");
    }

     [TestMethod]
    public void ProjectMonitoringSubscriptioAndProjectJohnDoeCallwebApIwithprojectid()
    {
      msg.Title("Project WebTest 6", "Project monitoring subscription and project JohnDoe asset. Get the project id for valid tag file request");
      var ts = new TestSupport { IsPublishToKafka = false };
      var legacyProjectId = ts.SetLegacyProjectId();
      var legacyAssetId = -1;
      var projectUid = Guid.NewGuid();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var deviceUid = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDate = ts.FirstEventDate.ToString("yyyy-MM-dd");
      var endDate = new DateTime(9999, 12, 31).ToString("yyyy-MM-dd");
      var geometryWKT = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      // Write events to database 
      var projectEventArray = new[] {
       "| TableName | EventDate   | ProjectUID   | LegacyProjectID   | Name            | fk_ProjectTypeID | ProjectTimeZone           | LandfillTimeZone | StartDate   | EndDate   | GeometryWKT   |",
      $"| Project   | 0d+09:00:00 | {projectUid} | {legacyProjectId} | ProjectWebTest6 | 2                | New Zealand Standard Time | Pacific/Auckland | {startDate} | {endDate} | {geometryWKT} |" };
      ts.PublishEventCollection(projectEventArray);
      var eventsArray = new[] {
         "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
        $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |",
        $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
        $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 20               | {startDate} | {endDate}      |               |          |                    |",
        $"| CustomerProject     | 0d+09:20:00 |               |           |                   |                   | {customerUid}  |                  |             |                | {projectUid}  |          |                    |",
        $"| ProjectSubscription | 0d+09:20:00 |               |           |                   |                   |                |                  | {startDate} |                | {projectUid}  |          | {subscriptionUid}  |"};
      ts.PublishEventCollection(eventsArray);
      var assetEventArray = new[] {
       "| TableName | EventDate   | AssetUID      | LegacyAssetID   | Name            | MakeCode | SerialNumber | Model | IconKey | AssetType  | OwningCustomerUID |",
      $"| Asset     | 0d+09:00:00 | {ts.AssetUid} | {legacyAssetId} | ProjectWebTest6 | CAT      | XAT1         | 345D  | 10      | Excavators | {customerUid}     |"};
      ts.PublishEventCollection(assetEventArray);
      var deviceEventArray = new[] {
       "| TableName         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | fk_AssetUID   | fk_DeviceUID | fk_SubscriptionUID | EffectiveDate | ",
      $"| Device            | 0d+09:00:00 | {deviceUid}        | Subscribed  | Series522  | {deviceUid} | CDMA         | ProjectWebTest6           |               |              |                    |               |",
      $"| AssetDevice       | 0d+09:20:00 |                    |             |            |             |              |                           | {ts.AssetUid} | {deviceUid}  |                    |               |",
      };
      ts.PublishEventCollection(deviceEventArray);

      var actualResult = CallWebApiGetProjectId(ts, -1, 38.837, -121.348, ts.FirstEventDate.AddDays(1), tccOrg.ToString());
      Assert.AreEqual(legacyProjectId, actualResult.projectId, " Legacy project id's do not match");
      Assert.AreEqual(true, actualResult.result, " result of request doesn't match expected");
    }

    [TestMethod]
    public void LandfillSubscriptioAndProjectJohnDoeCallwebApIwithprojectid()
    {
      msg.Title("Project WebTest 7", "Landfill subscription and project. Get the project id for valid tag file request");
      var ts = new TestSupport { IsPublishToKafka = false };
      var legacyProjectId = ts.SetLegacyProjectId();
      var legacyAssetId = -1;
      var projectUid = Guid.NewGuid();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var deviceUid = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDate = ts.FirstEventDate.ToString("yyyy-MM-dd");
      var endDate = new DateTime(9999, 12, 31).ToString("yyyy-MM-dd");
      var geometryWKT = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      // Write events to database 
      var projectEventArray = new[] {
       "| TableName | EventDate   | ProjectUID   | LegacyProjectID   | Name            | fk_ProjectTypeID | ProjectTimeZone           | LandfillTimeZone | StartDate   | EndDate   | GeometryWKT   |",
      $"| Project   | 0d+09:00:00 | {projectUid} | {legacyProjectId} | ProjectWebTest7 | 1                | New Zealand Standard Time | Pacific/Auckland | {startDate} | {endDate} | {geometryWKT} |" };
      ts.PublishEventCollection(projectEventArray);
      var eventsArray = new[]
      {
         "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
        $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |",
        $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
        $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                    |",
        $"| CustomerProject     | 0d+09:20:00 |               |           |                   |                   | {customerUid}  |                  |             |                | {projectUid}  |          |                    |",
        $"| ProjectSubscription | 0d+09:20:00 |               |           |                   |                   |                |                  | {startDate} |                | {projectUid}  |          | {subscriptionUid}  |"
      };
      ts.PublishEventCollection(eventsArray);
      var assetEventArray = new[] {
       "| TableName | EventDate   | AssetUID      | LegacyAssetID   | Name            | MakeCode | SerialNumber | Model | IconKey | AssetType  | OwningCustomerUID |",
      $"| Asset     | 0d+09:00:00 | {ts.AssetUid} | {legacyAssetId} | ProjectWebTest7 | CAT      | XAT1         | 345D  | 10      | Excavators | {customerUid}     |"};
      ts.PublishEventCollection(assetEventArray);
      var deviceEventArray = new[] {
       "| TableName         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | fk_AssetUID   | fk_DeviceUID | fk_SubscriptionUID | EffectiveDate | ",
      $"| Device            | 0d+09:00:00 | {deviceUid}        | Subscribed  | Series522  | {deviceUid} | CDMA         | ProjectWebTest7           |               |              |                    |               |",
      $"| AssetDevice       | 0d+09:20:00 |                    |             |            |             |              |                           | {ts.AssetUid} | {deviceUid}  |                    |               |",
      };
      ts.PublishEventCollection(deviceEventArray);

      var actualResult = CallWebApiGetProjectId(ts, -1, 38.837, -121.348, ts.FirstEventDate.AddDays(1), tccOrg.ToString());
      Assert.AreEqual(legacyProjectId, actualResult.projectId, " Legacy project id's do not match");
      Assert.AreEqual(true, actualResult.result, " result of request doesn't match expected");
    }

     [TestMethod]
    public void ProjectMonitoringSubscriptioAndProjectManualImportCallwebApIwithprojectid()
    {
      msg.Title("Project WebTest 8", "Project monitoring subscription and project with Manual Import. Get the project id for valid tag file request");
      var ts = new TestSupport { IsPublishToKafka = false };
      var legacyProjectId = ts.SetLegacyProjectId();
      var legacyAssetId = -2;
      var projectUid = Guid.NewGuid();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var deviceUid = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDate = ts.FirstEventDate.ToString("yyyy-MM-dd");
      var endDate = new DateTime(9999, 12, 31).ToString("yyyy-MM-dd");
      var geometryWKT = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      // Write events to database 
      var projectEventArray = new[] {
       "| TableName | EventDate   | ProjectUID   | LegacyProjectID   | Name            | fk_ProjectTypeID | ProjectTimeZone           | LandfillTimeZone | StartDate   | EndDate   | GeometryWKT   |",
      $"| Project   | 0d+09:00:00 | {projectUid} | {legacyProjectId} | ProjectWebTest8 | 2                | New Zealand Standard Time | Pacific/Auckland | {startDate} | {endDate} | {geometryWKT} |" };
      ts.PublishEventCollection(projectEventArray);
      var eventsArray = new[]
      {
         "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
        $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |",
        $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
        $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 20               | {startDate} | {endDate}      |               |          |                    |",
        $"| CustomerProject     | 0d+09:20:00 |               |           |                   |                   | {customerUid}  |                  |             |                | {projectUid}  |          |                    |",
        $"| ProjectSubscription | 0d+09:20:00 |               |           |                   |                   |                |                  | {startDate} |                | {projectUid}  |          | {subscriptionUid}  |"
      };
      ts.PublishEventCollection(eventsArray);
      var assetEventArray = new[] {
       "| TableName | EventDate   | AssetUID      | LegacyAssetID   | Name            | MakeCode | SerialNumber | Model | IconKey | AssetType  | OwningCustomerUID |",
      $"| Asset     | 0d+09:00:00 | {ts.AssetUid} | {legacyAssetId} | ProjectWebTest8 | CAT      | XAT1         | 345D  | 10      | Excavators | {customerUid}     |"};
      ts.PublishEventCollection(assetEventArray);
      var deviceEventArray = new[] {
       "| TableName         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | fk_AssetUID   | fk_DeviceUID | fk_SubscriptionUID | EffectiveDate | ",
      $"| Device            | 0d+09:00:00 | {deviceUid}        | Subscribed  | Series522  | {deviceUid} | CDMA         | ProjectWebTest8           |               |              |                    |               |",
      $"| AssetDevice       | 0d+09:20:00 |                    |             |            |             |              |                           | {ts.AssetUid} | {deviceUid}  |                    |               |",
      $"| AssetSubscription | 0d+09:20:00 |                    |             |            |             |              |                           | {ts.AssetUid} |              | {subscriptionUid}  | {startDate}   |"};
      ts.PublishEventCollection(deviceEventArray);

      var actualResult = CallWebApiGetProjectId(ts, -2, 38.837, -121.348, ts.FirstEventDate.AddDays(1), tccOrg.ToString());
      Assert.AreEqual(-1, actualResult.projectId, " Legacy project id's do not match");
      Assert.AreEqual(false, actualResult.result, " result of request doesn't match expected");
    }

    [TestMethod]
    public void LandfillSubscriptioAndProjectManualImportCallwebApIwithprojectid()
    {
      msg.Title("Project WebTest 9", "Landfill subscription and project with Manual Import. Get the project id for valid tag file request");
      var ts = new TestSupport { IsPublishToKafka = false };
      var legacyProjectId = ts.SetLegacyProjectId();
      var legacyAssetId = -2;
      var projectUid = Guid.NewGuid();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var deviceUid = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDate = ts.FirstEventDate.ToString("yyyy-MM-dd");
      var endDate = new DateTime(9999, 12, 31).ToString("yyyy-MM-dd");
      var geometryWKT = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      // Write events to database 
      var projectEventArray = new[] {
       "| TableName | EventDate   | ProjectUID   | LegacyProjectID   | Name            | fk_ProjectTypeID | ProjectTimeZone           | LandfillTimeZone | StartDate   | EndDate   | GeometryWKT   |",
      $"| Project   | 0d+09:00:00 | {projectUid} | {legacyProjectId} | ProjectWebTest9 | 1                | New Zealand Standard Time | Pacific/Auckland | {startDate} | {endDate} | {geometryWKT} |" };
      ts.PublishEventCollection(projectEventArray);
      var eventsArray = new[] {
         "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
        $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |",
        $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
        $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                    |",
        $"| CustomerProject     | 0d+09:20:00 |               |           |                   |                   | {customerUid}  |                  |             |                | {projectUid}  |          |                    |",
        $"| ProjectSubscription | 0d+09:20:00 |               |           |                   |                   |                |                  | {startDate} |                | {projectUid}  |          | {subscriptionUid}  |"};
      ts.PublishEventCollection(eventsArray);
      var assetEventArray = new[] {
       "| TableName | EventDate   | AssetUID      | LegacyAssetID   | Name            | MakeCode | SerialNumber | Model | IconKey | AssetType  | OwningCustomerUID |",
      $"| Asset     | 0d+09:00:00 | {ts.AssetUid} | {legacyAssetId} | ProjectWebTest9 | CAT      | XAT1         | 345D  | 10      | Excavators | {customerUid}     |"};
      ts.PublishEventCollection(assetEventArray);
      var deviceEventArray = new[] {
       "| TableName         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | fk_AssetUID   | fk_DeviceUID | fk_SubscriptionUID | EffectiveDate | ",
      $"| Device            | 0d+09:00:00 | {deviceUid}        | Subscribed  | Series522  | {deviceUid} | CDMA         | ProjectWebTest9           |               |              |                    |               |",
      $"| AssetDevice       | 0d+09:20:00 |                    |             |            |             |              |                           | {ts.AssetUid} | {deviceUid}  |                    |               |"};
      ts.PublishEventCollection(deviceEventArray);
      var actualResult = CallWebApiGetProjectId(ts, -2, 38.837, -121.348, ts.FirstEventDate.AddDays(1), tccOrg.ToString());
      Assert.AreEqual(legacyProjectId, actualResult.projectId, " Legacy project id's do not match");
      Assert.AreEqual(true, actualResult.result, " result of request doesn't match expected");
    }


    [TestMethod]
    public void ValidateProjectGetIdMessage1()
    {
      msg.Title("Project Validate 1", "Validate error message : success");
      var ts = new TestSupport {IsPublishToKafka = false};
      var legacyAssetId = ts.SetLegacyAssetId();
      var tccOrg = Guid.NewGuid();
      ts = CreateTestDataForProjectTest(ts, legacyAssetId, tccOrg);

      var actualResult = CallWebApiGetProjectId(ts, -1, 38.837, -121.348, ts.FirstEventDate.AddDays(1),tccOrg.ToString());
      Assert.AreEqual("success", actualResult.Message, " result message from web api does not match expected");
    }

    [TestMethod]
    public void ValidateProjectGetIdMessage2()
    {
      msg.Title("Project Validate 2", "Validate error message : Must contain one or more of assetId or tccOrgId");
      var ts = new TestSupport {IsPublishToKafka = false};
      var legacyAssetId = ts.SetLegacyAssetId();
      var tccOrg = Guid.NewGuid();
      ts = CreateTestDataForProjectTest(ts, legacyAssetId, tccOrg);

      var actualResult = CallWebApiGetProjectIdBadresponse(ts, 0, 38.837, -121.348, ts.FirstEventDate.AddDays(1), string.Empty);
      Assert.AreEqual("Must contain one or more of assetId 0 or tccOrgId ", actualResult.Message, " result message from web api does not match expected");
      Assert.AreEqual(ContractExecutionStatesEnum.ValidationError, actualResult.Code, "code from web api does not match expected");
    }

    [TestMethod]
    public void ValidateProjectGetIdMessage3()
    {
      msg.Title("Project Validate 3", "Validate error message : Latitude value of should be between -90 degrees and 90 degrees");
      var ts = new TestSupport {IsPublishToKafka = false};
      var legacyAssetId = ts.SetLegacyAssetId();
      var tccOrg = Guid.NewGuid();
      ts = CreateTestDataForProjectTest(ts, legacyAssetId, tccOrg);
      var actualResult = CallWebApiGetProjectIdBadresponse(ts, legacyAssetId, 138, -121.348, ts.FirstEventDate.AddDays(1), tccOrg.ToString());
      Assert.AreEqual("Latitude value of 138 should be between -90 degrees and 90 degrees", actualResult.Message, " result message from web api does not match expected");
      Assert.AreEqual(ContractExecutionStatesEnum.ValidationError, actualResult.Code, "code from web api does not match expected");
}
    [TestMethod]
    public void ValidateProjectGetIdMessage4()
    {
      msg.Title("Project Validate 4", "Validate error message : Latitude value of should be between -90 degrees and 90 degrees");
      var ts = new TestSupport {IsPublishToKafka = false};
      var legacyAssetId = ts.SetLegacyAssetId();
      var tccOrg = Guid.NewGuid();
      ts = CreateTestDataForProjectTest(ts, legacyAssetId, tccOrg);

      var actualResult = CallWebApiGetProjectIdBadresponse(ts, legacyAssetId, -138, -121.348, ts.FirstEventDate.AddDays(1), tccOrg.ToString());
      Assert.AreEqual("Latitude value of -138 should be between -90 degrees and 90 degrees", actualResult.Message, " result message from web api does not match expected");
      Assert.AreEqual(ContractExecutionStatesEnum.ValidationError, actualResult.Code, "code from web api does not match expected");
    }

    [TestMethod]
    public void ValidateProjectGetIdMessage5()
    {
      msg.Title("Project Validate 5", "Validate error message : Longitude value of should be between -180 degrees and 180 degrees");
      var ts = new TestSupport {IsPublishToKafka = false};
      var legacyAssetId = ts.SetLegacyAssetId();
      var tccOrg = Guid.NewGuid();
      ts = CreateTestDataForProjectTest(ts, legacyAssetId, tccOrg);
      var actualResult = CallWebApiGetProjectIdBadresponse(ts, legacyAssetId, 38.837, -221.348, ts.FirstEventDate.AddDays(1), tccOrg.ToString());
      Assert.AreEqual("Longitude value of -221.348 should be between -180 degrees and 180 degrees", actualResult.Message, " result message from web api does not match expected");
      Assert.AreEqual(ContractExecutionStatesEnum.ValidationError, actualResult.Code, "code from web api does not match expected");
    }
 
    [TestMethod]
    public void ValidateProjectGetIdMessage6()
    {
      msg.Title("Project Validate 6", "Validate error message : Longitude value of should be between -180 degrees and 180 degrees");
      var ts = new TestSupport {IsPublishToKafka = false};
      var legacyAssetId = ts.SetLegacyAssetId();
      var tccOrg = Guid.NewGuid();
      ts = CreateTestDataForProjectTest(ts, legacyAssetId, tccOrg);

      var actualResult =CallWebApiGetProjectIdBadresponse(ts, legacyAssetId, 38.837, 221.348, ts.FirstEventDate.AddDays(1),tccOrg.ToString());
      Assert.AreEqual("Longitude value of 221.348 should be between -180 degrees and 180 degrees",actualResult.Message, " result message from web api does not match expected");
      Assert.AreEqual(ContractExecutionStatesEnum.ValidationError, actualResult.Code, "code from web api does not match expected");
    }

    [TestMethod]
    public void ValidateProjectGetIdMessage7()
    {
      msg.Title("Project Validate 7", "Validate error message : timeOfPosition must have occured within last 5 years");
      var ts = new TestSupport {IsPublishToKafka = false};
      var legacyAssetId = ts.SetLegacyAssetId();
      var tccOrg = Guid.NewGuid();
      ts = CreateTestDataForProjectTest(ts, legacyAssetId, tccOrg);

      var actualResult = CallWebApiGetProjectIdBadresponse(ts, legacyAssetId, 38.837, -121.348, new DateTime(2012, 1, 1),tccOrg.ToString());
      Assert.AreEqual("timeOfPosition must have occured within last 5 years 01/01/2012 00:00:00", actualResult.Message," result message from web api does not match expected");
      Assert.AreEqual(ContractExecutionStatesEnum.ValidationError, actualResult.Code, "code from web api does not match expected");
    }

    [TestMethod]
    public void ValidateProjectGetIdMessage8()
    {
      msg.Title("Project Validate 8", "Validate error message : timeOfPosition must have occured within last 5 years");
      var ts = new TestSupport {IsPublishToKafka = false};
      var legacyAssetId = ts.SetLegacyAssetId();
      var tccOrg = Guid.NewGuid();
      ts = CreateTestDataForProjectTest(ts, legacyAssetId, tccOrg);
      var actualResult = CallWebApiGetProjectIdBadresponse(ts, legacyAssetId, 38.837, -121.348, new DateTime(9999,1,1), tccOrg.ToString());
      Assert.AreEqual("timeOfPosition must have occured within last 5 years 01/01/9999 00:00:00", actualResult.Message, " result message from web api does not match expected");
      Assert.AreEqual(ContractExecutionStatesEnum.ValidationError, actualResult.Code, "code from web api does not match expected");   
    }

    /// <summary>
    /// USed to tests the web api validation rules
    /// </summary>
    /// <param name="ts">Test support instance</param>
    /// <param name="legacyAssetId">Legacy asset id</param>
    /// <param name="tccOrg">tcc org id</param>
    /// <returns></returns>
    private TestSupport CreateTestDataForProjectTest(TestSupport ts, long legacyAssetId, Guid tccOrg)
    {
      var legacyProjectId = ts.SetLegacyProjectId();      
      var projectUid = Guid.NewGuid();
      var customerUid = Guid.NewGuid();

      var deviceUid = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDate = ts.FirstEventDate.ToString("yyyy-MM-dd");
      var endDate = new DateTime(9999, 12, 31).ToString("yyyy-MM-dd");
      var geometryWKT =
        "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      // Write events to database 
      var projectEventArray = new[]
      {
        "| TableName | EventDate   | ProjectUID   | LegacyProjectID   | Name            | fk_ProjectTypeID | ProjectTimeZone           | LandfillTimeZone | StartDate   | EndDate   | GeometryWKT   |",
       $"| Project   | 0d+09:00:00 | {projectUid} | {legacyProjectId} | ProjectWebTest1 | 0                | New Zealand Standard Time | Pacific/Auckland | {startDate} | {endDate} | {geometryWKT} |"
      };
      ts.PublishEventCollection(projectEventArray);
      var eventsArray = new[]
      {
         "| TableName       | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID |",
        $"| Customer        | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |",
        $"| CustomerTccOrg  | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |",
        $"| Subscription    | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 13               | {startDate} | {endDate}      |               |          |",
        $"| CustomerProject | 0d+09:20:00 |               |           |                   |                   | {customerUid}  |                  |             |                | {projectUid}  |          |"
      };
      ts.PublishEventCollection(eventsArray);
      var assetEventArray = new[]
      {
         "| TableName | EventDate   | AssetUID      | LegacyAssetID   | Name            | MakeCode | SerialNumber | Model | IconKey | AssetType  | OwningCustomerUID |",
        $"| Asset     | 0d+09:00:00 | {ts.AssetUid} | {legacyAssetId} | ProjectWebTest1 | CAT      | XAT1         | 345D  | 10      | Excavators | {customerUid}     |"
      };
      ts.PublishEventCollection(assetEventArray);
      var deviceEventArray = new[]
      {
         "| TableName         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | fk_AssetUID   | fk_DeviceUID | fk_SubscriptionUID | EffectiveDate | ",
        $"| Device            | 0d+09:00:00 | {deviceUid}        | Subscribed  | Series522  | {deviceUid} | CDMA         | ProjectWebTest1           |               |              |                    |               |",
        $"| AssetDevice       | 0d+09:20:00 |                    |             |            |             |              |                           | {ts.AssetUid} | {deviceUid}  |                    |               |",
        $"| AssetSubscription | 0d+09:20:00 |                    |             |            |             |              |                           | {ts.AssetUid} |              | {subscriptionUid}  | {startDate}   |"
      };
      ts.PublishEventCollection(deviceEventArray);
      return ts;
    }

    /// <summary>
    /// Call the get project request
    /// </summary>
    /// <param name="ts">test support</param>
    /// <param name="assetid">legacy asset ID (radio serial)</param>
    /// <param name="latitude">seed position latitude value from tagfile</param>
    /// <param name="longitude">seed position longitude value from tagfile</param>
    /// <param name="timeOfPosition">from tagfile-used to check against valid Project time range.</param>
    /// <param name="tccOrgUid">UID of the TCC account the VL customer is paired with. Identifies which VL customer projects to search.</param>
    /// <returns>The project id result</returns>
    private GetProjectIdResult CallWebApiGetProjectId(TestSupport ts,long assetid,double latitude,double longitude, DateTime timeOfPosition,string tccOrgUid)
    {
      var request = GetProjectIdRequest.CreateGetProjectIdRequest(assetid,latitude,longitude, 0, timeOfPosition,tccOrgUid);
      var requestJson = JsonConvert.SerializeObject(request, ts.jsonSettings);
      var restClient = new RestClient();
      var uri = ts.GetBaseUri() + "api/v1/project/getId";
      var method = HttpMethod.Post.ToString();
      var response = restClient.DoHttpRequest(uri, method, requestJson);
      var actualResult = JsonConvert.DeserializeObject<GetProjectIdResult>(response, ts.jsonSettings);
      return actualResult;
    }


    /// <summary>
    /// Call the get project request but expect a bad request as a result
    /// </summary>
    /// <param name="ts">test support</param>
    /// <param name="assetid">legacy asset ID (radio serial)</param>
    /// <param name="latitude">seed position latitude value from tagfile</param>
    /// <param name="longitude">seed position longitude value from tagfile</param>
    /// <param name="timeOfPosition">from tagfile-used to check against valid Project time range.</param>
    /// <param name="tccOrgUid">UID of the TCC account the VL customer is paired with. Identifies which VL customer projects to search.</param>
    /// <returns>The project id result</returns>
    private GetProjectIdResult CallWebApiGetProjectIdBadresponse(TestSupport ts,long assetid,double latitude,double longitude, DateTime timeOfPosition,string tccOrgUid)
    {
      var request = GetProjectIdRequest.CreateGetProjectIdRequest(assetid,latitude,longitude, 0, timeOfPosition,tccOrgUid);
      var requestJson = JsonConvert.SerializeObject(request, ts.jsonSettings);
      var restClient = new RestClient();
      var uri = ts.GetBaseUri() + "api/v1/project/getId";
      var method = HttpMethod.Post.ToString();
      var response = restClient.DoHttpRequest(uri, method, requestJson,System.Net.HttpStatusCode.BadRequest);
      var actualResult = JsonConvert.DeserializeObject<GetProjectIdResult>(response, ts.jsonSettings);
      return actualResult;
    }
  }
}

      
  