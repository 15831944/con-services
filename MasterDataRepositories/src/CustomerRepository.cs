﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.GenericConfiguration;
using Repositories.DBModels;

namespace Repositories
{
  public class CustomerRepository : RepositoryBase, IRepository<ICustomerEvent>
  {
    private readonly ILogger log;

    public CustomerRepository(IConfigurationStore _connectionString, ILoggerFactory logger) : base(_connectionString)
    {
      log = logger.CreateLogger<CustomerRepository>();
    }

    public async Task<int> StoreEvent(ICustomerEvent evt)
    {
      var upsertedCount = 0;

      if (evt is CreateCustomerEvent)
      {
        var customerEvent = (CreateCustomerEvent)evt;
        var customer = new Customer();
        customer.Name = customerEvent.CustomerName;
        customer.CustomerUID = customerEvent.CustomerUID.ToString();
        customer.CustomerType = (CustomerType)Enum.Parse(typeof(CustomerType), customerEvent.CustomerType, true);
        customer.LastActionedUTC = customerEvent.ActionUTC;
        upsertedCount = await UpsertCustomerDetail(customer, "CreateCustomerEvent");
      }
      else if (evt is UpdateCustomerEvent)
      {
        var customerEvent = (UpdateCustomerEvent)evt;
        var customer = new Customer();
        customer.Name = customerEvent.CustomerName;
        customer.CustomerUID = customerEvent.CustomerUID.ToString();
        customer.LastActionedUTC = customerEvent.ActionUTC;
        upsertedCount = await UpsertCustomerDetail(customer, "UpdateCustomerEvent");
      }
      else if (evt is DeleteCustomerEvent)
      {
        var customerEvent = (DeleteCustomerEvent)evt;
        var customer = new Customer();
        customer.CustomerUID = customerEvent.CustomerUID.ToString();
        customer.LastActionedUTC = customerEvent.ActionUTC;
        upsertedCount = await UpsertCustomerDetail(customer, "DeleteCustomerEvent");
      }
      else if (evt is AssociateCustomerUserEvent)
      {
        var customerEvent = (AssociateCustomerUserEvent)evt;
        var customerUser = new CustomerUser();
        customerUser.CustomerUID = customerEvent.CustomerUID.ToString();
        customerUser.UserUID = customerEvent.UserUID.ToString();
        customerUser.LastActionedUTC = customerEvent.ActionUTC;
        upsertedCount = await UpsertCustomerUserDetail(customerUser, "AssociateCustomerUserEvent");
      }
      else if (evt is DissociateCustomerUserEvent)
      {
        var customerEvent = (DissociateCustomerUserEvent)evt;
        var customerUser = new CustomerUser();
        customerUser.CustomerUID = customerEvent.CustomerUID.ToString();
        customerUser.UserUID = customerEvent.UserUID.ToString();
        customerUser.LastActionedUTC = customerEvent.ActionUTC;
        upsertedCount = await UpsertCustomerUserDetail(customerUser, "DissociateCustomerUserEvent");
      }

      return upsertedCount;
    }

    /// <summary>
    /// All Customer detail-related columns can be inserted, 
    /// but only certain columns can be updated.
    /// On the deletion, a corresponded entry will be deleted.
    /// </summary>
    /// <param name="customer"></param>
    /// <param name="eventType"></param>
    /// <returns></returns>
    private async Task<int> UpsertCustomerDetail(Customer customer, string eventType)
    {
      int upsertedCount = 0;

      await PerhapsOpenConnection();

      var existing = (await Connection.QueryAsync<Customer>
        (@"SELECT 
                CustomerUID, Name, fk_CustomerTypeID AS CustomerType, IsDeleted, LastActionedUTC
              FROM Customer
              WHERE CustomerUID = @CustomerUid",
        new { CustomerUid = customer.CustomerUID }
        )).FirstOrDefault();

      if (existing != null && existing.IsDeleted == true)
      {
        log.LogDebug("CustomerRepository/UpsertCustomerDetail No update as the Deleted Customer exists: customerUid:{0}", customer.CustomerUID);
        return upsertedCount;
      }

      if (eventType == "CreateCustomerEvent")
      {
        upsertedCount = await CreateCustomer(customer, existing);
      }

      if (eventType == "UpdateCustomerEvent")
      {
        upsertedCount = await UpdateCustomer(customer, existing);
      }

      if (eventType == "DeleteCustomerEvent")
      {
        upsertedCount = await DeleteCustomer(customer, existing);
      }

      PerhapsCloseConnection();
      return upsertedCount;
    }

    private async Task<int> CreateCustomer(Customer customer, Customer existing)
    {
      var upsertedCount = 0;
      if (existing == null)
      {
        log.LogDebug("CustomerRepository/CreateCustomer: going to create customer={0}", JsonConvert.SerializeObject(customer));

        const string insert =
          @"INSERT Customer
                (CustomerUID, Name, fk_CustomerTypeID, IsDeleted, LastActionedUTC)
              VALUES
                (@CustomerUID, @Name, @CustomerType, @IsDeleted, @LastActionedUTC)";

        return await dbAsyncPolicy.ExecuteAsync(async () =>
        {
          upsertedCount = await Connection.ExecuteAsync(insert, customer);
          log.LogDebug("CustomerRepository/CreateCustomer (Create/insert): upserted {0} rows (1=insert, 2=update) for: customerUid:{1}", upsertedCount, customer.CustomerUID);
          return upsertedCount == 2 ? 1 : upsertedCount; // 2=1RowUpdated; 1=1RowInserted; 0=noRowsInserted       
        });
      }
      else if (existing.LastActionedUTC >= customer.LastActionedUTC)
      {
        // must be a later update was applied before the create arrived
        // leave the more recent actionUTC alone

        log.LogDebug("CustomerRepository/CreateCustomer: going to update customer={0}", JsonConvert.SerializeObject(customer));
        const string update =
            @"UPDATE Customer                
                SET fk_CustomerTypeID = @CustomerType
                WHERE CustomerUID = @CustomerUID";

        return await dbAsyncPolicy.ExecuteAsync(async () =>
        {
          upsertedCount = await Connection.ExecuteAsync(update, customer);
          log.LogDebug("CustomerRepository/CreateCustomer: (Create/update): upserted {0} rows (1=insert, 2=update) for: customerUid:{1}", upsertedCount, customer.CustomerUID);
          return upsertedCount == 2 ? 1 : upsertedCount; // 2=1RowUpdated; 1=1RowInserted; 0=noRowsInserted       
        });
      }

      log.LogDebug("CustomerRepository/CreateCustomer: can't create as already exists customer={0}", JsonConvert.SerializeObject(customer));
      return upsertedCount;
    }

    private async Task<int> UpdateCustomer(Customer customer, Customer existing)
    {
      var upsertedCount = 0;
      if (existing != null)
      {
        if (customer.LastActionedUTC >= existing.LastActionedUTC)
        {
          log.LogDebug("CustomerRepository/UpdateCustomer: going to update customer={0}", JsonConvert.SerializeObject(customer));
          const string update =
            @"UPDATE Customer                
                SET Name = @Name,
                  LastActionedUTC = @LastActionedUTC
                WHERE CustomerUID = @CustomerUID";
          return await dbAsyncPolicy.ExecuteAsync(async () =>
          {
            upsertedCount = await Connection.ExecuteAsync(update, customer);
            log.LogDebug("CustomerRepository/UpdateCustomer: (update): upserted {0} rows (1=insert, 2=update) for: customerUid:{1}", upsertedCount, customer.CustomerUID);
            return upsertedCount == 2 ? 1 : upsertedCount; // 2=1RowUpdated; 1=1RowInserted; 0=noRowsInserted       
          });
        }
        else
        {
          log.LogDebug("CustomerRepository/UpdateCustomer: old ActionedUtc so ignored customer={0}", JsonConvert.SerializeObject(customer));
        }
      }
      else
      {
        log.LogDebug("CustomerRepository/UpdateCustomer: doesn't exist,going to add dummy customer={0}", JsonConvert.SerializeObject(customer));
        customer.CustomerType = CustomerType.Customer; // need a default
        const string insert =
          @"INSERT Customer
                (CustomerUID, Name, fk_CustomerTypeID, LastActionedUTC)
              VALUES
                (@CustomerUID, @Name, @CustomerType, @LastActionedUTC)";
        return await dbAsyncPolicy.ExecuteAsync(async () =>
        {
          upsertedCount = await Connection.ExecuteAsync(insert, customer);
          log.LogDebug("CustomerRepository/UpdateCustomer (insert): upserted {0} rows (1=insert, 2=update) for: customerUid:{1}", upsertedCount, customer.CustomerUID);
          return upsertedCount == 2 ? 1 : upsertedCount; // 2=1RowUpdated; 1=1RowInserted; 0=noRowsInserted       
        });
      }

      return upsertedCount;
    }

    private async Task<int> DeleteCustomer(Customer customer, Customer existing)
    {
      var upsertedCount = 0;
      if (existing == null)
      {
          log.LogDebug("CustomerRepository/DeleteCustomer: inserting a deleted customer={0}", JsonConvert.SerializeObject(customer));

          customer.CustomerType = CustomerType.Customer; // need a default
          customer.Name = "";
          customer.IsDeleted = true;
          const string insert =
            @"INSERT Customer
                (CustomerUID, Name, fk_CustomerTypeID, IsDeleted, LastActionedUTC)
              VALUES
                (@CustomerUID, @Name, @CustomerType, @IsDeleted, @LastActionedUTC)";
          return await dbAsyncPolicy.ExecuteAsync(async () =>
          {
            upsertedCount = await Connection.ExecuteAsync(insert, customer);
            log.LogDebug("CustomerRepository/DeleteCustomer: (insert): upserted {0} rows (1=insert, 2=update) for: customerUid:{1}", upsertedCount, customer.CustomerUID);
            return upsertedCount == 2 ? 1 : upsertedCount; // 2=1RowUpdated; 1=1RowInserted; 0=noRowsInserted       
        });
      }
      else
      {
        if (customer.LastActionedUTC >= existing.LastActionedUTC )
        {
          log.LogDebug("CustomerRepository/DeleteCustomer: updating to deleted customer={0}", JsonConvert.SerializeObject(customer));

          const string update =
            @"UPDATE Customer                
                SET IsDeleted = 1,
                  LastActionedUTC = @LastActionedUTC                
                WHERE CustomerUID = @CustomerUID";
          return await dbAsyncPolicy.ExecuteAsync(async () =>
          {
            upsertedCount = await Connection.ExecuteAsync(update, customer);
            log.LogDebug("CustomerRepository/DeleteCustomer: (update): upserted {0} rows (1=insert, 2=update) for: customerUid:{1}", upsertedCount, customer.CustomerUID);
            return upsertedCount == 2 ? 1 : upsertedCount; // 2=1RowUpdated; 1=1RowInserted; 0=noRowsInserted       
          });
        }
        else
        {
          log.LogDebug("CustomerRepository/DeleteCustomer: old delete event, ignore customer={0}", JsonConvert.SerializeObject(customer));
          return upsertedCount;
        }
      }            
      
    }

    public async Task<Customer> GetAssociatedCustomerbyUserUid(System.Guid userUid)
    {
      await PerhapsOpenConnection();

      var customer = (await Connection.QueryAsync<Customer>
          (@"SELECT CustomerUID, Name, fk_CustomerTypeID AS CustomerType, IsDeleted, c.LastActionedUTC 
                FROM Customer c 
                JOIN CustomerUser cu ON cu.fk_CustomerUID = c.CustomerUID 
                WHERE cu.UserUID = @userUid AND c.IsDeleted = 0",
            new { userUid = userUid.ToString() }
            )).FirstOrDefault();

      PerhapsCloseConnection();

      return customer;
    }

    public async Task<Customer> GetCustomer(System.Guid customerUid)
    {
      await PerhapsOpenConnection();

      var customer = (await Connection.QueryAsync<Customer>
          (@"SELECT CustomerUID, Name, fk_CustomerTypeID AS CustomerType, IsDeleted, LastActionedUTC 
                FROM Customer 
                WHERE CustomerUID = @customerUid AND IsDeleted = 0",
             new { customerUid = customerUid.ToString() }
             )).FirstOrDefault();

      PerhapsCloseConnection();

      return customer;
    }

    /// <summary>
    /// All CustomerUser detail-related columns can be inserted, 
    /// but only certain columns can be updated.
    /// On the deletion, a corresponded entry will be deleted.
    /// </summary>
    /// <param name="customerUser"></param>
    /// <param name="eventType"></param>
    /// <returns></returns>
    private async Task<int> UpsertCustomerUserDetail(CustomerUser customerUser, string eventType)
    {
      int upsertedCount = 0;

      await PerhapsOpenConnection();

      var existing = (await Connection.QueryAsync<CustomerUser>
        (@"SELECT 
                UserUID, fk_CustomerUID AS CustomerUID, LastActionedUTC
              FROM CustomerUser
              WHERE fk_CustomerUID = @customerUID AND UserUID = @userUID",
          new { customerUID = customerUser.CustomerUID, userUID = customerUser.UserUID }
          )).FirstOrDefault();

      if (eventType == "AssociateCustomerUserEvent")
      {
        upsertedCount = await AssociateCustomerUser(customerUser, existing);
      }

      if (eventType == "DissociateCustomerUserEvent")
      {
        upsertedCount = await DissociateCustomerUser(customerUser, existing);
      }

      PerhapsCloseConnection();
      return upsertedCount;
    }

    private async Task<int> AssociateCustomerUser(CustomerUser customerUser, CustomerUser existing)
    {
      var upsertedCount = 0;
      if (existing == null)
      {
        log.LogDebug("CustomerRepository/AssociateCustomerUser: inserting a customerUser={0}", JsonConvert.SerializeObject(customerUser));
        const string insert =
          @"INSERT CustomerUser
              (UserUID, fk_CustomerUID, LastActionedUTC)
            VALUES
              (@UserUID, @CustomerUID, @LastActionedUTC)";

        return await dbAsyncPolicy.ExecuteAsync(async () =>
        {
          upsertedCount = await Connection.ExecuteAsync(insert, customerUser);
           log.LogDebug("CustomerRepository/AssociateCustomerUser: upserted {0} rows (1=insert, 2=update) for: customerUid:{1}", upsertedCount, customerUser.CustomerUID);
          return upsertedCount == 2 ? 1 : upsertedCount; // 2=1RowUpdated; 1=1RowInserted; 0=noRowsInserted       
        });
      }

      //      Log.DebugFormat("CustomerRepository: can't create as already exists newActionedUTC={0}", customerUser.LastActionedUTC);
      return upsertedCount;
    }

    private async Task<int> DissociateCustomerUser(CustomerUser customerUser, CustomerUser existing)
    {
      var upsertedCount = 0;
      if (existing != null)
      {
        if (customerUser.LastActionedUTC >= existing.LastActionedUTC)
        {
          log.LogDebug("CustomerRepository/DissociateCustomerUser: deleting a customerUser={0}", JsonConvert.SerializeObject(customerUser));
          const string delete =
            @"DELETE 
                FROM CustomerUser               
                WHERE fk_CustomerUID = @CustomerUID AND UserUID = @UserUID";
          return await dbAsyncPolicy.ExecuteAsync(async () =>
          {
            upsertedCount = await Connection.ExecuteAsync(delete, customerUser);
            log.LogDebug("CustomerRepository/DissociateCustomerUser: upserted {0} rows (1=insert, 2=update) for: customerUid:{1}", upsertedCount, customerUser.CustomerUID);
            return upsertedCount == 2 ? 1 : upsertedCount; // 2=1RowUpdated; 1=1RowInserted; 0=noRowsInserted       
          });
        }

        // may have been associated again since, so don't delete
        log.LogDebug("CustomerRepository/DissociateCustomerUser: old delete event ignored customerUser={0}", JsonConvert.SerializeObject(customerUser));
      }
      else
      {
        log.LogDebug("CustomerRepository/DissociateCustomerUser: can't delete as none existing customerUser={0}", JsonConvert.SerializeObject(customerUser));        
      }
      return upsertedCount;
    }


    public async Task<Customer> GetCustomer_UnitTest(System.Guid customerUid)
    {
      await PerhapsOpenConnection();

      var customer = (await Connection.QueryAsync<Customer>
          (@"SELECT CustomerUID, Name, fk_CustomerTypeID AS CustomerType, IsDeleted, LastActionedUTC 
                FROM Customer 
                WHERE CustomerUID = @customerUid",
             new { customerUid = customerUid.ToString() }
             )).FirstOrDefault();

      PerhapsCloseConnection();

      return customer;
    }

    public async Task<CustomerUser> GetAssociatedCustomerbyUserUid_UnitTest(System.Guid userUid)
    {
      await PerhapsOpenConnection();

      var customer = (await Connection.QueryAsync<CustomerUser>
          (@"SELECT fk_CustomerUID AS CustomerUID, UserUID, LastActionedUTC 
                FROM CustomerUser
                WHERE UserUID = @userUid",
            new { userUid = userUid.ToString() }
            )).FirstOrDefault();

      PerhapsCloseConnection();

      return customer;
    }
  }
}
