﻿using Autofac;
using log4net;
using System;
using System.Configuration;
using System.Net;
using System.Reflection;
using Topshelf;
using Topshelf.Runtime;
using VSS.UserCustomer.Data;
using VSS.UserCustomer.Data.Interfaces;
using VSS.Kafka.DotNetClient.Model;
using VSS.UserCustomer.Processor.Consumer;
using VSS.UserCustomer.Processor.Interfaces;

namespace VSS.UserCustomer.Processor
{
  internal class Program
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    protected static IContainer Container { get; set; }

    public static void Main(string[] args)
    {
      TopshelfExitCode exitCode = HostFactory.Run(c =>
      {
        c.SetServiceName("_UserCustomerMasterDataConsumer");
        c.SetDisplayName("_UserCustomerMasterDataConsumer");
        c.SetDescription("Service for processing user customer master data payloads from Kafka");

        c.RunAsLocalSystem();
        c.StartAutomatically();
        c.EnableServiceRecovery(cfg =>
        {
          cfg.RestartService(1);
          cfg.RestartService(1);
          cfg.RestartService(1);
        });
        c.Service<ServiceController>(s =>
        {
          s.ConstructUsing(ServiceFactory);
          s.WhenStarted(o => { o.Start(); });
          s.WhenStopped(o => { o.Stop(); });
        });
      });
    }

    private static ServiceController ServiceFactory(HostSettings settings)
    {
      Log.Debug("UserCustomerProcessor: starting ServiceFactory");

      var builder = new ContainerBuilder();
      builder.RegisterType<ServiceController>()
        .AsSelf()
        .SingleInstance();

      string confluentBaseUrl = ConfigurationManager.AppSettings["KafkaServerUri"];
      if (string.IsNullOrWhiteSpace(confluentBaseUrl))
        throw new ArgumentNullException("RestProxy Base Url is empty");

      string kafkaTopicName = Settings.Default.TopicName;
      string consumerGroupName = Settings.Default.ConsumerGroupName;

      builder.Register(config =>
      {
        var consumerConfigurator = new ConsumerConfigurator(confluentBaseUrl, kafkaTopicName, consumerGroupName,
          Dns.GetHostName(), 1024);
        return consumerConfigurator;
      }).As<IConsumerConfigurator>().SingleInstance();

      builder.RegisterType<UserCustomerProcessor>().As<IUserCustomerProcessor>().SingleInstance();
      builder.RegisterType<UserCustomerEventObserver>().As<IObserver<ConsumerInstanceResponse>>().SingleInstance();
      builder.RegisterType<MySqlUserCustomerRepository>().As<IUserCustomerService>().SingleInstance();

      Container = builder.Build();
      return Container.Resolve<ServiceController>();
    }
  }
}
