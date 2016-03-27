﻿using log4net;
using System;
using System.Reflection;
using VSS.Subscription.Processor.Interfaces;

namespace VSS.Subscription.Processor
{
  public class ServiceController
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private readonly ISubscriptionProcessor _subscriptionProcessor;

    public ServiceController(ISubscriptionProcessor subscriptionProcessor)
    {
        _subscriptionProcessor = subscriptionProcessor;
    }

    public bool Start()
    {
      try
      {
        _subscriptionProcessor.Process();
      }
      catch (Exception ex)
      {
        Log.Info(string.Format("Failed to start Subscription Processor.. \n {0} \n {1}", ex.Message, ex.StackTrace));
        return false;
      }
      Log.Info("Subscription Processor has been Started");
      return true;
    }

    public void Stop()
    {
      _subscriptionProcessor.Stop();
      Log.InfoFormat("SubscriptionProcessor has been Stopped");
    }

    public void Error()
    {
      Log.InfoFormat("SubscriptionProcessor has thrown an error");
    }
  }
}
