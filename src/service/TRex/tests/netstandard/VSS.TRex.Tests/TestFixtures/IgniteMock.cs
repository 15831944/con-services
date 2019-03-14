﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Binary;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Cluster;
using Apache.Ignite.Core.Compute;
using Apache.Ignite.Core.Messaging;
using Moq;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Common.Serialisation;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.SiteModels.GridFabric.Events;
using VSS.TRex.SiteModels.Interfaces.Events;
using VSS.TRex.SubGrids.GridFabric.Listeners;
using VSS.TRex.Tests.BinarizableSerialization;

namespace VSS.TRex.Tests.TestFixtures
{
  /// <summary>
  /// Defines a collection of Mock objects that collectively mock and replumb the Ignite instructure layer into a form suitable for unit/integration testing
  /// with frameworks like XUnit and NUnit with the TRex business logic not being aware it is not running on an actual Ignite grid.
  /// </summary>
  public class IgniteMock
  {
    public Mock<ICompute> mockCompute { get; }
    public Mock<IClusterNode> mockClusterNode { get; }
    public Mock<ICollection<IClusterNode>> mockClusterNodes { get; }
    public Mock<IMessaging> mockMessaging { get; }
    public Mock<IClusterGroup> mockClusterGroup { get; }
    public Mock<ICluster> mockCluster { get; }
    public Mock<IIgnite> mockIgnite { get; }

    /// <summary>
    /// Constructor that creates the collection of mocks that together mock the Ignite infrastructure layer in TRex
    /// </summary>
    public IgniteMock()
    {
      // Wire up Ignite Compute Apply and Broadcast apis on the Compute interface
      mockCompute = new Mock<ICompute>();

      // Pretend there is a single node in the cluster group
      mockClusterNode = new Mock<IClusterNode>();
      mockClusterNode.Setup(x => x.GetAttribute<string>("TRexNodeId")).Returns("UnitTest-TRexNodeId");

      mockClusterNodes = new Mock<ICollection<IClusterNode>>();
      mockClusterNodes.Setup(x => x.Count).Returns(1);

      // Set up the Ignite message fabric mocks to plumb sender and receiver together
      var messagingDictionary = new Dictionary<object, object>(); // topic => listener

      mockMessaging = new Mock<IMessaging>();
      mockMessaging
        .Setup(x => x.LocalListen(It.IsAny<IMessageListener<byte[]>>(), It.IsAny<object>()))
        .Callback((IMessageListener<byte[]> listener, object topic) =>
        {
          messagingDictionary.Add(topic, listener);
        });

      mockMessaging
        .Setup(x => x.LocalListen(It.IsAny<IMessageListener<ISiteModelAttributesChangedEvent>>(), It.IsAny<object>()))
        .Callback((IMessageListener<ISiteModelAttributesChangedEvent> listener, object topic) =>
        {
          messagingDictionary.Add(topic, listener);
        });

      mockMessaging
        .Setup(x => x.Send(It.IsAny<object>(), It.IsAny<object>()))
        .Callback((object message, object topic) =>
        {
          messagingDictionary.TryGetValue(topic, out var listener);
          if (listener is SubGridListener _listener)
            _listener.Invoke(Guid.Empty, message as byte[]);
          else
            throw new TRexException($"Type of listener ({listener}) not SubGridListener as expected.");
        });

      mockMessaging
        .Setup(x => x.SendOrdered(It.IsAny<object>(), It.IsAny<object>(), It.IsAny<TimeSpan?>()))
        .Callback((object message, object topic, TimeSpan? timeSpan) =>
        {
          messagingDictionary.TryGetValue(topic, out var listener);
          if (listener is SubGridListener _listener1)
            _listener1.Invoke(Guid.Empty, message as byte[]);
          else if (listener is SiteModelAttributesChangedEventListener _listener2)
            _listener2.Invoke(Guid.Empty, message as SiteModelAttributesChangedEvent);
          else
            throw new TRexException($"Type of listener not SubGridListener or SiteModelAttributesChangedEventListener as expected.");
        });

      mockClusterGroup = new Mock<IClusterGroup>();
      mockClusterGroup.Setup(x => x.GetNodes()).Returns(mockClusterNodes.Object);
      mockClusterGroup.Setup(x => x.GetCompute()).Returns(mockCompute.Object);
      mockClusterGroup.Setup(x => x.GetMessaging()).Returns(mockMessaging.Object);

      mockCompute.Setup(x => x.ClusterGroup).Returns(mockClusterGroup.Object);

      mockCluster = new Mock<ICluster>();
      mockCluster.Setup(x => x.ForAttribute(It.IsAny<string>(), It.IsAny<string>())).Returns(mockClusterGroup.Object);
      mockCluster.Setup(x => x.GetLocalNode()).Returns(mockClusterNode.Object);
      mockCluster.Setup(x => x.GetMessaging()).Returns(mockMessaging.Object);

      var clusterActiveState = true;
      mockCluster.Setup(x => x.IsActive()).Returns(() => clusterActiveState);
      mockCluster.Setup(x => x.SetActive(It.IsAny<bool>())).Callback((bool state) => { /* Never change state from true... clusterActiveState = state; */ });

      mockIgnite = new Mock<IIgnite>();
      mockIgnite.Setup(x => x.GetCluster()).Returns(mockCluster.Object);
      mockIgnite.Setup(x => x.GetMessaging()).Returns(mockMessaging.Object);
      mockIgnite.Setup(x => x.Name).Returns(TRexGrids.ImmutableGridName);
    }

    /// <summary>
    /// Removes and recreates any dynamic content contained in the Ignite mock. References to the mocked Ignite context are accessed via the TRex
    /// Depenedency Injection layer.
    /// </summary>
    public static void ResetDynamicMockedIgniteContent()
    {
      // Create the dictionary to contain all the mocked caches
      var cacheDictionary = new Dictionary<string, object>(); // object = ICache<TK, TV>

      // Create he mocked cache for the existence maps cache and any other cache using this signature
      var mockIgnite = DIContext.Obtain<Mock<IIgnite>>();

      mockIgnite.Setup(x => x.GetOrCreateCache<INonSpatialAffinityKey, byte[]>(It.IsAny<CacheConfiguration>())).Returns((CacheConfiguration cfg) =>
      {
        if (cacheDictionary.TryGetValue(cfg.Name, out var cache))
          return (ICache<INonSpatialAffinityKey, byte[]>)cache;

        var mockCache = new Mock<ICache<INonSpatialAffinityKey, byte[]>>();
        var mockCacheDictionary = new Dictionary<INonSpatialAffinityKey, byte[]>();

        mockCache.Setup(x => x.Get(It.IsAny<INonSpatialAffinityKey>())).Returns((INonSpatialAffinityKey key) =>
        {
          if (mockCacheDictionary.TryGetValue(key, out var value))
            return value;
          throw new KeyNotFoundException($"Key {key} not found in mock cache");
        });

        mockCache.Setup(x => x.Put(It.IsAny<INonSpatialAffinityKey>(), It.IsAny<byte[]>())).Callback((INonSpatialAffinityKey key, byte[] value) =>
        {
          mockCacheDictionary.Add(key, value);
        });

        cacheDictionary.Add(cfg.Name, mockCache.Object);
        return mockCache.Object;
      });
    }

    private static void TestIBinarizableSerializationForItem(object item)
    {
      if (item is IBinarizable)
      {
        // exercise serialize/deserialize of func and argument before invoking function
        var serializer = new BinarizableSerializer();

        var writer = new TestBinaryWriter();
        serializer.WriteBinary(item, writer);

        var newInstance = Activator.CreateInstance(item.GetType());

        serializer.ReadBinary(newInstance, new TestBinaryReader(writer._stream.BaseStream as MemoryStream));
      }
    }

    public static void AddApplicationGridRouting<TCompute, TArgument, TResponse>() where TCompute : IComputeFunc<TArgument, TResponse>
    {
      var mockCompute = DIContext.Obtain<Mock<ICompute>>();

      mockCompute.Setup(x => x.Apply(It.IsAny<TCompute>(), It.IsAny<TArgument>())).Returns((TCompute func, TArgument argument) =>
      {
        // exercise serialize/deserialize of func and argument before invoking function
        TestIBinarizableSerializationForItem(func);
        TestIBinarizableSerializationForItem(argument);
        var response = func.Invoke(argument);

        // exercise serialie/deserialise of response returning it
        TestIBinarizableSerializationForItem(response);
        return response;
      });

      mockCompute.Setup(x => x.ApplyAsync(It.IsAny<TCompute>(), It.IsAny<TArgument>())).Returns((TCompute func, TArgument argument) =>
      {
        // exercise serialize/deserialize of func and argument before invoking function
        TestIBinarizableSerializationForItem(func);
        TestIBinarizableSerializationForItem(argument);

        var task = new Task<TResponse>(() =>
        {
          var response = func.Invoke(argument);
          TestIBinarizableSerializationForItem(response);

          return response;
        });
        task.Start();

        return task;
      });
    }

    public static void AddClusterComputeGridRouting<TCompute, TArgument, TResponse>() where TCompute : IComputeFunc<TArgument, TResponse>
    {
      var mockCompute = DIContext.Obtain<Mock<ICompute>>();
      mockCompute.Setup(x => x.Broadcast(It.IsAny<TCompute>(), It.IsAny<TArgument>())).Returns((TCompute func, TArgument argument) =>
      {
        // exercise serialize/deserialize of func and argument before invoking function
        TestIBinarizableSerializationForItem(func);
        TestIBinarizableSerializationForItem(argument);

        var response = new List<TResponse> { func.Invoke(argument) };

        if (response.Count == 1 && response[0] != null)
          TestIBinarizableSerializationForItem(response[0]);

        return response;
      });

      mockCompute.Setup(x => x.BroadcastAsync(It.IsAny<TCompute>(), It.IsAny<TArgument>())).Returns((TCompute func, TArgument argument) =>
      {
        // exercise serialize/deserialize of func and argument before invoking function
        TestIBinarizableSerializationForItem(func);
        TestIBinarizableSerializationForItem(argument);

        var task = new Task<ICollection<TResponse>>(() =>
        {
          var response = func.Invoke(argument);
          TestIBinarizableSerializationForItem(response);

          return new List<TResponse> { response };
        });

        task.Start();
        return task;
      });
    }

    public static void AddClusterComputeSpatialAffinityGridRouting<TCompute, TArgument, TResponse>() where TCompute : IComputeFunc<TResponse>, IComputeFuncArgument<TArgument>
    {
      var mockCompute = DIContext.Obtain<Mock<ICompute>>();

      mockCompute.Setup(x => x.AffinityCall(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TCompute>())).Returns((string cacheName, object key, TCompute func) =>
      {
        // exercise serialize/deserialize of func and argument before invoking function
        TestIBinarizableSerializationForItem(func);
        TestIBinarizableSerializationForItem(key);

        var response = func.Invoke();

        TestIBinarizableSerializationForItem(response);

        return response;
      });

      mockCompute.Setup(x => x.AffinityCallAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TCompute>())).Returns((string cacheName, object key, TCompute func) =>
      {
        // exercise serialize/deserialize of func and argument before invoking function
        TestIBinarizableSerializationForItem(func);
        TestIBinarizableSerializationForItem(key);

        var response = func.Invoke();

        TestIBinarizableSerializationForItem(response);

        return Task.FromResult(response);
      });
    }
  }
}