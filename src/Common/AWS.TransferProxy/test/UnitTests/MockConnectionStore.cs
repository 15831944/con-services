﻿using System;
using Microsoft.Extensions.Configuration;
using VSS.Common.Abstractions.Configuration;

namespace VSS.AWS.TransferProxy.UnitTests
{
  public class MockConnectionStore : IConfigurationStore
  {
    public string GetValueString(string v)
    {
      return v;
    }

    public string GetValueString(string v, string defaultValue)
    {
      return defaultValue;
    }

    public bool? GetValueBool(string v)
    {
      throw new NotImplementedException();
    }

    public bool GetValueBool(string v, bool defaultValue)
    {
      return defaultValue;
    }

    public int GetValueInt(string v)
    {
      throw new NotImplementedException();
    }

    public int GetValueInt(string v, int defaultValue)
    {
      return defaultValue;
    }

    public uint GetValueUint(string v)
    {
      throw new NotImplementedException();
    }

    public uint GetValueUint(string v, uint defaultValue)
    {
      return defaultValue;
    }

    public long GetValueLong(string v)
    {
      throw new NotImplementedException();
    }

    public long GetValueLong(string v, long defaultValue)
    {
      return defaultValue;
    }

    public ulong GetValueUlong(string v)
    {
      throw new NotImplementedException();
    }

    public ulong GetValueUlong(string v, ulong defaultValue)
    {
      return defaultValue;
    }

    public double GetValueDouble(string v)
    {
      throw new NotImplementedException();
    }

    public double GetValueDouble(string v, double defaultValue)
    {
      return defaultValue;
    }

    public TimeSpan? GetValueTimeSpan(string v)
    {
      return new TimeSpan();
    }

    public TimeSpan GetValueTimeSpan(string v, TimeSpan defaultValue)
    {
      return defaultValue;
    }

    public Guid GetValueGuid(string v)
    {
      throw new NotImplementedException();
    }

    public Guid GetValueGuid(string v, Guid defaultValue)
    {
      return defaultValue;
    }

    public string GetConnectionString(string connectionType)
    {
      throw new NotImplementedException();
    }

    public string GetConnectionString(string connectionType, string databaseNameKey)
    {
      throw new NotImplementedException();
    }

    public IConfigurationSection GetSection(string key)
    {
      throw new NotImplementedException();
    }

    public IConfigurationSection GetLoggingConfig()
    {
      throw new NotImplementedException();
    }

    public DateTime? GetValueDateTime(string key)
    {
      throw new NotImplementedException();
    }

    public DateTime GetValueDateTime(string key, DateTime defaultValue)
    {
      throw new NotImplementedException();
    }

    public bool UseKubernetes => false;
    public string KubernetesConfigMapName => string.Empty;
    public string KubernetesNamespace => string.Empty;
  }
}
