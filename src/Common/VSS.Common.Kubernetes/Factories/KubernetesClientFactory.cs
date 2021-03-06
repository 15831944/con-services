﻿using System;
using k8s;
using k8s.Exceptions;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Kubernetes.Interfaces;
using VSS.ConfigurationStore;
using YamlDotNet.Core;

namespace VSS.Common.Kubernetes.Factories
{
  public class KubernetesClientFactory : IKubernetesClientFactory
  {
    private readonly ILogger<KubernetesClientFactory> logger;
    private readonly IConfigurationStore configuration;
    private KubernetesClientConfiguration kubernetesConfiguration;

    private readonly string defaultNamespace;

    private string ClusterNamespace =>
      kubernetesConfiguration == null || string.IsNullOrEmpty(kubernetesConfiguration.Namespace)
        ? defaultNamespace
        : kubernetesConfiguration.Namespace;

    public KubernetesClientFactory(ILogger<KubernetesClientFactory> logger, IConfigurationStore configuration)
    {
      this.logger = logger;
      this.configuration = configuration;
      var environment = configuration.GetValueString("ENVIRONMENT", "default");
      defaultNamespace = configuration.GetValueString("kubernetesNamespace", environment);
    }

    public (IKubernetes client, string currentNamespace) CreateClient(string kubernetesContext = null)
    {
      if (kubernetesConfiguration != null)
      {
        return (new k8s.Kubernetes(kubernetesConfiguration), ClusterNamespace);
      }

      if (string.IsNullOrWhiteSpace(kubernetesContext))
      {
        try
        {
          kubernetesConfiguration = KubernetesClientConfiguration.InClusterConfig();
          logger.LogInformation("Using In Cluster Config");
        }
        catch (KubeConfigException configException)
        {
          try
          {
            logger.LogWarning($"Cannot get InClusterConfig, using the config file. Error: {configException.Message}");
            kubernetesConfiguration = KubernetesClientConfiguration.BuildConfigFromConfigFile();
          }
          catch (Exception genericException)
          {
            // We will get this exception when we can't find the ~/.kube/config file
            // Which is valid in prod, as they should be using in cluster config
            if (genericException is KubeConfigException)
            {
              logger.LogWarning($"Cannot get ~/.kube/config file - giving up connecting to the cluster. Error: {genericException.Message}");
              return (null, null);
            }
#if DEBUG
            // This expcetion occurswhen we can't parse the file, due to custom authentication methods (such as we what we use for AWS)
            // (see https://github.com/kubernetes-client/csharp/issues/91#issuecomment-362920478)
            // We *should* only get this running locally (until the issue is fixed)
            // If this happens in prod it's bad, so don't handle it in prod
            else if(genericException is YamlException)
            {
              logger.LogError(genericException, "Failed to read the config file - see if https://github.com/kubernetes-client/csharp/issues/91#issuecomment-362920478 is resolved");
              // Run "kubectl proxy" from the commandline to the cluster you're interested in
              // This will just help track down service detection, but you won't be able to connect to the services
              kubernetesConfiguration = new KubernetesClientConfiguration {Host = "http://127.0.0.1:8001"};

            }
#endif
            else
            {
              throw;
            }
          }
        }
      }
      else
      {
        logger.LogInformation($"Using Context {kubernetesContext}");
        try
        {
          kubernetesConfiguration =
            KubernetesClientConfiguration.BuildConfigFromConfigFile(currentContext: kubernetesContext);
        }
        catch (KubeConfigException)
        {
          logger.LogWarning("Cannot get ~/.kube/config file - giving up connecting to the cluster");
          return (null, null);
        }
      }

      return (new k8s.Kubernetes(kubernetesConfiguration), ClusterNamespace);
    }
  }
}
