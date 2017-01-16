﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using KafkaConsumer;
using Newtonsoft.Json;

using Microsoft.Extensions.Logging;
using VSS.Subscription.Data.Models;
using VSS.Project.Service.Utils;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Project.Service.Repositories
{

  public class SubscriptionRepository : RepositoryBase, IRepository<ISubscriptionEvent>
  {
    public SubscriptionRepository(IConfigurationStore connectionString)
        : base(connectionString)
    {

    }

    public Dictionary<string, VSS.Subscription.Data.Models.ServiceType> _serviceTypes = null;


    public async Task<int> StoreEvent(ISubscriptionEvent evt)
    {
      var upsertedCount = 0;

      if (_serviceTypes == null)
        _serviceTypes = (await GetServiceTypes()).ToDictionary(k => k.Name, v => v);

      if (evt is CreateProjectSubscriptionEvent)
      {
        var subscriptionEvent = (CreateProjectSubscriptionEvent)evt;
        var subscription = new VSS.Subscription.Data.Models.Subscription();
        subscription.SubscriptionUID = subscriptionEvent.SubscriptionUID.ToString();
        subscription.CustomerUID = subscriptionEvent.CustomerUID.ToString();
        subscription.ServiceTypeID = _serviceTypes[subscriptionEvent.SubscriptionType].ID;
        subscription.StartDate = subscriptionEvent.StartDate;
        //This is to handle CG subscriptions where we set the EndDate annually.
        //In NG the end date is the maximum unless it is cancelled/terminated.
        subscription.EndDate = subscriptionEvent.EndDate > DateTime.UtcNow ? new DateTime(9999, 12, 31) : subscriptionEvent.EndDate;
        subscription.LastActionedUTC = subscriptionEvent.ActionUTC;
        upsertedCount = await UpsertSubscriptionDetail(subscription, "CreateProjectSubscriptionEvent");
      }
      else if (evt is UpdateProjectSubscriptionEvent)
      {
        var subscriptionEvent = (UpdateProjectSubscriptionEvent)evt;
        var subscription = new VSS.Subscription.Data.Models.Subscription();
        subscription.SubscriptionUID = subscriptionEvent.SubscriptionUID.ToString();

        // this is dangerous. I suppose if current logic is chnanged to MOVE a servicePlan for rental customers
        // i.e. from one to the next customer, then this may be possible.
        //   in that scenario, what should be the relavant StartDate, EndDate and EffectiveDate? (not of concern here)
        subscription.CustomerUID = subscriptionEvent.CustomerUID.HasValue ? subscriptionEvent.CustomerUID.Value.ToString() : null;

        // should not be able to change a serviceType!!!
        // subscription.ServiceTypeID = _serviceTypes[subscriptionEvent.SubscriptionType].ID;
        subscription.StartDate = subscriptionEvent.StartDate ?? DateTime.MinValue;
        // todo update allows a future endDate but create does not, is this an error?
        // also, for both create and update for start and end dates these are calendar days
        //    in the assets timezone, but the create checks for UTC time....
        subscription.EndDate = subscriptionEvent.EndDate ?? DateTime.MinValue;
        subscription.LastActionedUTC = subscriptionEvent.ActionUTC;
        upsertedCount = await UpsertSubscriptionDetail(subscription, "UpdateProjectSubscriptionEvent");
      }
      else if (evt is AssociateProjectSubscriptionEvent)
      {
        var subscriptionEvent = (AssociateProjectSubscriptionEvent)evt;
        var projectSubscription = new VSS.Subscription.Data.Models.ProjectSubscription();
        projectSubscription.SubscriptionUID = subscriptionEvent.SubscriptionUID.ToString();
        projectSubscription.ProjectUID = subscriptionEvent.ProjectUID.ToString();
        projectSubscription.EffectiveDate = subscriptionEvent.EffectiveDate;
        projectSubscription.LastActionedUTC = subscriptionEvent.ActionUTC;
        upsertedCount = await UpsertProjectSubscriptionDetail(projectSubscription, "AssociateProjectSubscriptionEvent");
      }
      else if (evt is CreateCustomerSubscriptionEvent)
      {
        var subscriptionEvent = (CreateCustomerSubscriptionEvent)evt;
        var subscription = new VSS.Subscription.Data.Models.Subscription();
        subscription.SubscriptionUID = subscriptionEvent.SubscriptionUID.ToString();
        subscription.CustomerUID = subscriptionEvent.CustomerUID.ToString();
        subscription.ServiceTypeID = _serviceTypes[subscriptionEvent.SubscriptionType].ID;
        subscription.StartDate = subscriptionEvent.StartDate;
        //This is to handle CG subscriptions where we set the EndDate annually.
        //In NG the end date is the maximum unless it is cancelled/terminated.
        subscription.EndDate = subscriptionEvent.EndDate > DateTime.UtcNow ? new DateTime(9999, 12, 31) : subscriptionEvent.EndDate;
        subscription.LastActionedUTC = subscriptionEvent.ActionUTC;
        upsertedCount = await UpsertSubscriptionDetail(subscription, "CreateCustomerSubscriptionEvent");
      }
      else if (evt is UpdateCustomerSubscriptionEvent)
      {
        var subscriptionEvent = (UpdateCustomerSubscriptionEvent)evt;
        var subscription = new VSS.Subscription.Data.Models.Subscription();
        subscription.SubscriptionUID = subscriptionEvent.SubscriptionUID.ToString();
        subscription.StartDate = subscriptionEvent.StartDate ?? DateTime.MinValue;
        subscription.EndDate = subscriptionEvent.EndDate ?? DateTime.MinValue;
        subscription.LastActionedUTC = subscriptionEvent.ActionUTC;
        upsertedCount = await UpsertSubscriptionDetail(subscription, "UpdateCustomerSubscriptionEvent");
      }

      return upsertedCount;
    }



    /// <summary>
    /// All detail-related columns can be inserted, 
    ///    but only certain columns can be updated.
    /// </summary>
    /// <param name="subscription"></param>
    /// <param name="eventType"></param>
    /// <returns>Number of upserted records</returns>
    private async Task<int> UpsertSubscriptionDetail(VSS.Subscription.Data.Models.Subscription subscription, string eventType)
    {
      int upsertedCount = 0;

      await PerhapsOpenConnection();

      //       Log.DebugFormat("SubscriptionRepository: Upserting eventType={0} SubscriptionUID={1}", eventType, subscription.SubscriptionUID);

      var existing = Connection.Query<VSS.Subscription.Data.Models.Subscription>
        (@"SELECT 
                SubscriptionUID, fk_CustomerUID AS CustomerUID, StartDate, EndDate, fk_ServiceTypeID AS ServiceTypeID, LastActionedUTC 
              FROM Subscription
              WHERE SubscriptionUID = @subscriptionUID",
          new { subscriptionUID = subscription.SubscriptionUID }).FirstOrDefault();

      if (eventType == "CreateProjectSubscriptionEvent" || eventType == "CreateCustomerSubscriptionEvent")
      {
        upsertedCount = await CreateProjectSubscription(subscription, existing);
      }

      if (eventType == "UpdateProjectSubscriptionEvent" || eventType == "UpdateCustomerSubscriptionEvent")
      {
        upsertedCount = await UpdateProjectSubscription(subscription, existing);
      }

      //    Log.DebugFormat("SubscriptionRepository: upserted {0} rows", upsertedCount);

      PerhapsCloseConnection();

      return upsertedCount;
    }

    private async Task<int> CreateProjectSubscription(VSS.Subscription.Data.Models.Subscription subscription, VSS.Subscription.Data.Models.Subscription existing)
    {
      if (existing == null)
      {
        const string insert =
          @"INSERT Subscription
                (SubscriptionUID, fk_CustomerUID, StartDate, EndDate, fk_ServiceTypeID, LastActionedUTC)
              VALUES
                (@SubscriptionUID, @CustomerUID, @StartDate, @EndDate, @ServiceTypeID, @LastActionedUTC)";
        return Connection.Execute(insert, subscription);
      }
      //     Log.DebugFormat("SubscriptionRepository: can't create as already exists newActionedUTC {0}. So, the existing entry should be updated.", subscription.LastActionedUTC);
      return 0;
    }

    private async Task<int> UpdateProjectSubscription(VSS.Subscription.Data.Models.Subscription subscription, VSS.Subscription.Data.Models.Subscription existing)
    {
      // todo this code allows customerUID and serviceType to be updated - is this intentional?
      if (existing != null)
      {
        if (subscription.LastActionedUTC >= existing.LastActionedUTC)
        {
          //subscription only has values for columns to be updated
          if (string.IsNullOrEmpty(subscription.CustomerUID))
            subscription.CustomerUID = existing.CustomerUID;
          if (subscription.StartDate == DateTime.MinValue)
            subscription.StartDate = existing.StartDate;
          if (subscription.EndDate == DateTime.MinValue)
            subscription.EndDate = existing.EndDate;
          if (subscription.ServiceTypeID == 0)
            subscription.ServiceTypeID = existing.ServiceTypeID;

          const string update =
            @"UPDATE Subscription                
                  SET SubscriptionUID = @SubscriptionUID,
                      fk_CustomerUID = @CustomerUID,
                      StartDate=@StartDate, 
                      EndDate=@EndDate, 
                      fk_ServiceTypeID=@ServiceTypeID,
                      LastActionedUTC=@LastActionedUTC
                WHERE SubscriptionUID = @SubscriptionUID";
          return Connection.Execute(update, subscription);
        }

        //          Log.DebugFormat("SubscriptionRepository: old update event ignored currentActionedUTC{0} newActionedUTC{1}",
        //             existing.LastActionedUTC, subscription.LastActionedUTC);
      }
      else
      {
        //        Log.DebugFormat("SubscriptionRepository: can't update as none existing newActionedUTC {0}",
        //          subscription.LastActionedUTC);
      }
      return 0;
    }

    private async Task<int> UpsertProjectSubscriptionDetail(VSS.Subscription.Data.Models.ProjectSubscription projectSubscription, string eventType)
    {
      int upsertedCount = 0;

      await PerhapsOpenConnection();

      //    Log.DebugFormat("SubscriptionRepository: Upserting eventType={0} ProjectUid={1}, SubscriptionUid={2}",
      //     eventType, projectSubscription.ProjectUID, projectSubscription.SubscriptionUID);

      var existing = Connection.Query<VSS.Subscription.Data.Models.ProjectSubscription>
          (@"SELECT 
                fk_SubscriptionUID AS SubscriptionUID, fk_ProjectUID AS ProjectUID, EffectiveDate, LastActionedUTC
              FROM ProjectSubscription
              WHERE fk_ProjectUID = @projectUID AND fk_SubscriptionUID = @subscriptionUID",
          new { projectUID = projectSubscription.ProjectUID, subscriptionUID = projectSubscription.SubscriptionUID }).FirstOrDefault();

      if (eventType == "AssociateProjectSubscriptionEvent")
      {
        upsertedCount = await AssociateProjectSubscription(projectSubscription, existing);
      }

      //     Log.DebugFormat("SubscriptionRepository: upserted {0} rows", upsertedCount);

      PerhapsCloseConnection();

      return upsertedCount;
    }

    private async Task<int> AssociateProjectSubscription(VSS.Subscription.Data.Models.ProjectSubscription projectSubscription, VSS.Subscription.Data.Models.ProjectSubscription existing)
    {
      await PerhapsOpenConnection();
      if (existing == null)
      {
        const string insert =
          @"INSERT ProjectSubscription
                (fk_SubscriptionUID, fk_ProjectUID, EffectiveDate, LastActionedUTC)
              VALUES
                (@SubscriptionUID, @ProjectUID, @EffectiveDate, @LastActionedUTC)";

        PerhapsCloseConnection();
        return Connection.Execute(insert, projectSubscription);
      }
      PerhapsCloseConnection();
      //        Log.DebugFormat("SubscriptionRepository: can't create as already exists newActionedUTC={0}", projectSubscription.LastActionedUTC);
      return 0;
    }

    private async Task<IEnumerable<ServiceType>> GetServiceTypes()
    {
      await PerhapsOpenConnection();

      //     Log.Debug("SubscriptionRepository: Getting service types");

      var serviceTypes = Connection.Query<VSS.Subscription.Data.Models.ServiceType>
          (@"SELECT 
                s.ID, s.Description AS Name, sf.ID AS ServiceTypeFamilyID, sf.Description AS ServiceTypeFamilyName
              FROM ServiceTypeEnum s JOIN ServiceTypeFamilyEnum sf on s.fk_ServiceTypeFamilyID = sf.ID"
          );

      PerhapsCloseConnection();

      return serviceTypes;
    }

    public async Task<Subscription.Data.Models.Subscription> GetSubscription(string subscriptionUid)
    {
      await PerhapsOpenConnection();

      var subscription = Connection.Query<VSS.Subscription.Data.Models.Subscription>
        (@"SELECT 
                SubscriptionUID, fk_CustomerUID AS CustomerUID, fk_ServiceTypeID AS ServiceTypeID, StartDate, EndDate, LastActionedUTC
              FROM Subscription
              WHERE SubscriptionUID = @subscriptionUid"
          , new { subscriptionUid }
        ).FirstOrDefault();

      PerhapsCloseConnection();

      return subscription;
    }

    // todo this must be internal for unit testing?
    public async Task<IEnumerable<Subscription.Data.Models.Subscription>> GetSubscriptions_UnitTest(string subscriptionUid)
    {
      await PerhapsOpenConnection();

      var subscriptions = Connection.Query<VSS.Subscription.Data.Models.Subscription>
        (@"SELECT 
                SubscriptionUID, fk_CustomerUID AS CustomerUID, fk_ServiceTypeID AS ServiceTypeID, StartDate, EndDate, LastActionedUTC
              FROM Subscription
              WHERE SubscriptionUID = @subscriptionUid"
          , new { subscriptionUid }
         );

      PerhapsCloseConnection();

      return subscriptions;
    }

    public async Task<IEnumerable<Subscription.Data.Models.ProjectSubscription>> GetProjectSubscriptions_UnitTest(string subscriptionUid)
    {
      int upsertedCount = 0;

      await PerhapsOpenConnection();

      //    Log.DebugFormat("SubscriptionRepository: Upserting eventType={0} ProjectUid={1}, SubscriptionUid={2}",
      //     eventType, projectSubscription.ProjectUID, projectSubscription.SubscriptionUID);

      var projectSubscriptions = Connection.Query<VSS.Subscription.Data.Models.ProjectSubscription>
          (@"SELECT 
                fk_SubscriptionUID AS SubscriptionUID, fk_ProjectUID AS ProjectUID, EffectiveDate, LastActionedUTC
              FROM ProjectSubscription
              WHERE fk_SubscriptionUID = @subscriptionUID"
            , new { subscriptionUid }
          );

      PerhapsCloseConnection();

      return projectSubscriptions;
    }


  }

}