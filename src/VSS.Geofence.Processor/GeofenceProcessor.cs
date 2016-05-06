﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using java.util;
using log4net;
using System;
using System.Reflection;
using org.apache.kafka.clients.consumer;
using VSS.Geofence.Data.Interfaces;
using VSS.Geofence.Processor.Interfaces;
using VSS.Geofence.Processor.Properties;
using Random = System.Random;

namespace VSS.Geofence.Processor
{
  public class GeofenceProcessor : IGeofenceProcessor
  {
    private readonly GeofenceEventObserver _subscriber;
    private readonly KafkaConsumer javaConsumer;

    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    public GeofenceProcessor(IGeofenceService service)
    {
        try
        {
        log4net.Config.XmlConfigurator.Configure();

        var random = new Random();

        var props = new java.util.Properties();
        props.put("bootstrap.servers", Settings.Default.KafkaUri);
        props.put("client.id", "11111");
        props.put("group.id", Settings.Default.ConsumerGroupName);
        props.put("enable.auto.commit", "false");
        props.put("auto.commit.interval.ms", "1000");
        props.put("session.timeout.ms", "30000");
        props.put("key.deserializer", "org.apache.kafka.common.serialization.StringDeserializer");
        props.put("value.deserializer", "org.apache.kafka.common.serialization.StringDeserializer");
        props.put("auto.offset.reset", "earliest");
        props.put("fetch.min.bytes", "10000");
        props.put("receive.buffer.bytes", "214400");
        props.put("max.partition.fetch.bytes", "571520");
        props.put("heartbeat.interval.ms", "1000");
        javaConsumer = new KafkaConsumer(props);

        _subscriber = new GeofenceEventObserver(service);
      }
      catch (Exception error)
      {
        Log.Error("Error creating the consumer" + error.Message + error.StackTrace, error);
      }
    }

    public void Process()
    {
      javaConsumer.subscribe(Arrays.asList(Settings.Default.TopicName));
      var consumingThread = new Thread(JavaConsumerWorker);
      consumingThread.Start();   
    }

    private void JavaConsumerWorker()
    {
      var buffer = new List<ConsumerRecord>();
      const int minBatchSize = 1;

      while (true)
      {
        var records = javaConsumer.poll(3000);
        buffer.AddRange(records.Cast<ConsumerRecord>());
        Log.DebugFormat("Received {0} messages", buffer.Count);
        if (buffer.Count < 1) continue;

        Log.DebugFormat("Processing {0} messages", buffer.Count);
        foreach (var consumerRecord in buffer)
        {
          Log.DebugFormat("Processing messages partition {0} offset {1}", consumerRecord.partition(), consumerRecord.offset());

          _subscriber.OnNext(consumerRecord);
        }

        try
        {
          javaConsumer.commitSync();
          Log.InfoFormat("Committed offset {0} partition {1}", buffer[0].offset(), buffer[0].partition());
        }
        catch (CommitFailedException cfe)
        {
          // See: https://mail-archives.apache.org/mod_mbox/kafka-users/201601.mbox/%3CCAJDuW=Da9L0GLEp0WVFVpE0DnhaWH59xBCs2dN8yVsORKnFPGA@mail.gmail.com%3E
          Log.WarnFormat("Consider reducing size of processed batch: {0}", cfe);
        }

        buffer.Clear();
      }
    }

    public void Stop()
    {
      javaConsumer.close();
    }
  }
}
