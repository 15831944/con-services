﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;

namespace VSS.KafkaConsumer.Kafka
{
  public interface IKafka : IDisposable
  {
    string ConsumerGroup { get; set; }
    string OffsetReset { get; set; }
    string Uri { get; set; }
    bool EnableAutoCommit { get; set; }
    int Port { get; set; }
    void Subscribe(List<string> topics);
    Task<CommittedOffsets> Commit();
    void InitConsumer(IConfigurationStore configurationStore, string groupName = null, ILogger<IKafka> logger = null);
    void InitProducer(IConfigurationStore configurationStore);
    void Send(string topic, IEnumerable<KeyValuePair<string, string>> messagesToSendWithKeys);
    void Send(IEnumerable<KeyValuePair<string, KeyValuePair<string, string>>> topicMessagesToSendWithKeys);
    Message Consume(TimeSpan timeout);
    bool IsInitializedProducer { get; }
    bool IsInitializedConsumer { get; }
    Task Send(string topic, KeyValuePair<string, string> messageToSendWithKey);
  }

  public class Message
  {
    public IEnumerable<byte[]> payload { get; }
    public Error message { get; }
    public long offset { get; }
    public long partition { get; }

    public Message(IEnumerable<byte[]> payload, Error message, long offset=-1, long partition=-1 )
    {
      this.payload = payload;
      this.message = message;
      this.offset = offset;
      this.partition = partition;
    }
  }

  public enum Error
  {
    NO_ERROR,
    NO_DATA
  }
}
