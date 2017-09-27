﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Principal;
using System.Threading.Tasks;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Project.WebAPI.Common.Internal;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;
using VSS.MasterData.Project.WebAPI.Filters;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.MasterData.Project.WebAPI.Controllers
{
  /// <summary>
  /// Base for all Project v4 controllers
  /// </summary>
  public abstract class BaseController : Controller
  {
    /// <summary>
    /// todo this should be resolved with MIP story to set these per service.
    /// </summary>
    protected readonly int customErrorMessageOffset = 2000;

    /// <summary>
    /// Gets or sets the local log provider.
    /// </summary>
    private readonly ILogger log;

    /// <summary>
    /// Gets or sets the Service exception handler.
    /// </summary>
    protected readonly IServiceExceptionHandler serviceExceptionHandler;

    /// <summary>
    /// Gets or sets the Configuration Store. 
    /// </summary>
    protected readonly IConfigurationStore configStore;

    /// <summary>
    /// Gets or sets the Kafak consumer.
    /// </summary>
    protected readonly IKafka producer;

    /// <summary>
    /// Gets or sets the Kafka topic.
    /// </summary>
    protected readonly string kafkaTopicName;

    /// <summary>
    /// Gets or sets the Geofence proxy. 
    /// </summary>
    protected readonly IGeofenceProxy geofenceProxy;

    /// <summary>
    /// Gets or sets the Raptor proxy.
    /// </summary>
    protected readonly IRaptorProxy raptorProxy;

    /// <summary>
    /// Gets or sets the subscription proxy.
    /// </summary>
    protected readonly ISubscriptionProxy subscriptionProxy;

    /// <summary>
    /// Gets or sets the Project Repository. 
    /// </summary>
    protected readonly ProjectRepository projectRepo;

    /// <summary>
    /// Gets or sets the Subscription Repository.
    /// </summary>
    protected readonly SubscriptionRepository subscriptionRepo;

    /// <summary>
    /// Save for potential rollback
    /// </summary>
    protected Guid subscriptionUidAssigned = Guid.Empty;

    /// <summary>
    ///
    /// </summary>
    protected Guid geofenceUidCreated = Guid.Empty;

    /// <summary>
    /// Gets the custom headers for the request.
    /// </summary>
    /// <value>
    /// The custom headers.
    /// </value>
    protected IDictionary<string, string> customHeaders => Request.Headers.GetCustomHeaders();

    /// <summary>
    /// Gets the customer uid from the current context
    /// </summary>
    /// <value>
    /// The customer uid.
    /// </value>
    protected string customerUid => GetCustomerUid();

    /// <summary>
    /// Gets the user id from the current context
    /// </summary>
    /// <value>
    /// The user uid or applicationID as a string.
    /// </value>
    protected string userId => GetUserId();

    /// <summary>
    /// Gets the userEmailAddress from the current context
    /// </summary>
    /// <value>
    /// The userEmailAddress.
    /// </value>
    protected string userEmailAddress => GetUserEmailAddress();

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseController"/> class.
    /// </summary>
    /// <param name="log"></param>
    /// <param name="configStore">The configStore.</param>
    /// <param name="serviceExceptionHandler">The ServiceException handler</param>
    /// <param name="producer">The producer.</param>
    /// <param name="geofenceProxy">The geofence proxy.</param>
    /// <param name="raptorProxy">The raptorServices proxy.</param>
    /// <param name="subscriptionProxy">The subs proxy.</param>
    /// <param name="projectRepo">The project repo.</param>
    /// <param name="subscriptionsRepo">The subscriptions repo.</param>
    protected BaseController(ILogger log, IConfigurationStore configStore,
      IServiceExceptionHandler serviceExceptionHandler, IKafka producer,
      IGeofenceProxy geofenceProxy, IRaptorProxy raptorProxy, ISubscriptionProxy subscriptionProxy,
      IRepository<IProjectEvent> projectRepo, IRepository<ISubscriptionEvent> subscriptionsRepo)
    {
      this.log = log;
      this.configStore = configStore;
      this.serviceExceptionHandler = serviceExceptionHandler;
      this.producer = producer;

      if (!this.producer.IsInitializedProducer)
      {
        this.producer.InitProducer(configStore);
      }

      kafkaTopicName = "VSS.Interfaces.Events.MasterData.IProjectEvent" +
                       configStore.GetValueString("KAFKA_TOPIC_NAME_SUFFIX");

      this.projectRepo = projectRepo as ProjectRepository;
      subscriptionRepo = subscriptionsRepo as SubscriptionRepository;

      this.subscriptionProxy = subscriptionProxy;
      this.geofenceProxy = geofenceProxy;
      this.raptorProxy = raptorProxy;
    }

    /// <summary>
    /// With the service exception try execute.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action.</param>
    /// <returns></returns>
    protected async Task<TResult> WithServiceExceptionTryExecuteAsync<TResult>(Func
      <Task<TResult>> action)
      where TResult : ContractExecutionResult
    {
      TResult result = default(TResult);
      try
      {
        result = await action.Invoke().ConfigureAwait(false);
        log.LogTrace($"Executed {action.GetMethodInfo().Name} with result {JsonConvert.SerializeObject(result)}");
      }
      catch (ServiceException)
      {
        throw;
      }
      catch (Exception ex)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError,
          ContractExecutionStatesEnum.InternalProcessingError - customErrorMessageOffset, ex.Message);
      }
      finally
      {
        log.LogInformation($"Executed {action.GetMethodInfo().Name} with the result {result?.Code}");
      }

      return result;
    }

    /// <summary>
    /// Gets the customer uid from the context.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Incorrect customer uid value.</exception>
    private string GetCustomerUid()
    {
      if (User is TIDCustomPrincipal principal)
      {
        return principal.CustomerUid;
      }

      throw new ArgumentException("Incorrect customer in request context principal.");
    }

    /// <summary>
    /// Gets the User uid/applicationID from the context.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Incorrect user Id value.</exception>
    private string GetUserId()
    {
      if (User is TIDCustomPrincipal principal && (principal.Identity is GenericIdentity identity))
      {
        return identity.Name;
      }

      throw new ArgumentException("Incorrect UserId in request context principal.");
    }

    /// <summary>
    /// Gets the users email address from the context.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Incorrect email address value.</exception>
    private string GetUserEmailAddress()
    {
      if (User is TIDCustomPrincipal principal)
      {
        return principal.EmailAddress;
      }

      throw new ArgumentException("Incorrect user email address in request context principal.");
    }

    /// <summary>
    /// Gets the project.
    /// </summary>
    /// <param name="projectUid">The project uid.</param>
    protected async Task<Repositories.DBModels.Project> GetProject(string projectUid)
    {
      var customerUid = LogCustomerDetails("GetProject", projectUid);
      var project =
        (await projectRepo.GetProjectsForCustomer(customerUid).ConfigureAwait(false)).FirstOrDefault(
          p => string.Equals(p.ProjectUID, projectUid, StringComparison.OrdinalIgnoreCase));

      if (project == null)
      {
        log.LogWarning($"User doesn't have access to {projectUid}");
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.Forbidden, 1);
      }

      log.LogInformation($"Project {projectUid} retrieved");
      return project;
    }

    /// <summary>
    /// Log the Customer and Project details.
    /// </summary>
    /// <param name="functionName">Calling function name</param>
    /// <param name="projectUid">The Project Uid</param>
    /// <returns>Returns <see cref="TIDCustomPrincipal.CustomerUid"/></returns>
    protected string LogCustomerDetails(string functionName, string projectUid = "")
    {
      log.LogInformation(
        $"{functionName}: UserUID={GetUserId()}, CustomerUID={GetCustomerUid()}  and projectUid={projectUid}");

      return GetCustomerUid();
    }
  }
}