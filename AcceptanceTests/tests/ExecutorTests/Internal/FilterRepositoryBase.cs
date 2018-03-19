﻿using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using VSS.KafkaConsumer.Kafka;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace ExecutorTests.Internal
{
  public class FilterRepositoryBase : TestControllerBase
  {
    public void Setup()
    {
      SetupDI();
      Producer = ServiceProvider.GetRequiredService<IKafka>();
      if (!Producer.IsInitializedProducer)
      {
        Producer.InitProducer(ConfigStore);
      }

      KafkaTopicName = "VSS.Interfaces.Events.MasterData.IFilterEvent" +
                       ConfigStore.GetValueString("KAFKA_TOPIC_NAME_SUFFIX");
    }

    protected FilterRequestFull CreateAndValidateRequest(
      bool isApplicationContext = false,
      string userId = null,
      string projectUid = null,
      string filterUid = null,
      string name = null,
      string filterJson = "",
      string boundaryUid = null,
      string customerUid = null,
      FilterType filterType = FilterType.Transient,
      bool onlyFilterUid = false)
    {
      var request = FilterRequestFull.Create(
        new Dictionary<string, string>(),
        customerUid ?? Guid.NewGuid().ToString(),
        isApplicationContext,
        userId ?? Guid.NewGuid().ToString(),
        projectUid ?? Guid.NewGuid().ToString(),
        new FilterRequest
        {
          FilterUid = filterUid ?? Guid.NewGuid().ToString(),
          Name = name,
          FilterType = filterType,
          FilterJson = filterJson
        });

      request.Validate(ServiceExceptionHandler, onlyFilterUid);

      return request;
    }
  }
}