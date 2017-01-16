﻿using System;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Project.Data;
using VSS.Project.Data.Models;
using VSS.Customer.Data.Models;

namespace MasterDataConsumer.Tests
{
  [TestClass]
  public class CustomerEventsTests
  {
    [TestMethod]
    public void CustomerEventsCopyModels()
    {
      DateTime now = new DateTime(2017, 1, 1, 2, 30, 3);

      var customer = new Customer()
      {
        CustomerUID = Guid.NewGuid().ToString(),
        Name = "The Customer Name",
        CustomerType = CustomerType.Corporate,
        LastActionedUTC = now
      };

      var kafkaCustomerEvent = CopyModel(customer);
      var copiedCustomer = CopyModel(kafkaCustomerEvent);

      Assert.AreEqual(customer, copiedCustomer, "Customer model conversion not completed sucessfully");
    }

    #region private
    private CreateCustomerEvent CopyModel(Customer customer)
    {
      return new CreateCustomerEvent()
      {
        CustomerUID = Guid.Parse(customer.CustomerUID),
        CustomerName = customer.Name,
        CustomerType = customer.CustomerType.ToString(),
        ActionUTC = customer.LastActionedUTC
      };
    }

    private Customer CopyModel(CreateCustomerEvent kafkaCustomerEvent)
    {
      return new Customer()
      {
        CustomerUID = kafkaCustomerEvent.CustomerUID.ToString(),
        Name = kafkaCustomerEvent.CustomerName,
        CustomerType = (CustomerType)Enum.Parse(typeof(CustomerType), kafkaCustomerEvent.CustomerType, true),
        LastActionedUTC = kafkaCustomerEvent.ActionUTC
      };
    }
    #endregion

  }
}