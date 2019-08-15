﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.DataOcean.Client;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Interfaces.Repository;
using VSS.Serilog.Extensions;
using VSS.TCCFileAccess;
using VSS.WebApi.Common;
using ProjectDatabaseModel = VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels.Project;

namespace VSS.MasterData.Project.WebAPI.Controllers
{
  /// <summary>
  /// Base for all Project v4 controllers
  /// </summary>
  public abstract class BaseController : Controller
  {
    /// <summary>
    /// base message number for ProjectService
    /// </summary>
    protected readonly int customErrorMessageOffset = 2000;

    /// <summary>
    /// Gets or sets the local logger provider.
    /// </summary>
    protected readonly ILogger logger;

    /// <summary>
    /// Gets or sets the injected logger factory.
    /// </summary>
    protected readonly ILoggerFactory loggerFactory;

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
    /// Gets or sets the Raptor proxy.
    /// </summary>
    protected readonly IRaptorProxy raptorProxy;

    /// <summary>
    /// Gets or sets the Project Repository. 
    /// </summary>
    protected readonly IProjectRepository projectRepo;

    /// <summary>
    /// Gets or sets the Subscription Repository.
    /// </summary>
    protected readonly ISubscriptionRepository subscriptionRepo;

    /// <summary>
    /// Gets or sets the TCC File Repository.
    /// </summary>
    protected readonly IFileRepository fileRepo;

    /// <summary>
    /// Gets or sets the Data Ocean client agent.
    /// </summary>
    protected readonly IDataOceanClient dataOceanClient;

    /// <summary>
    /// Gets or sets the TPaaS application authentication helper.
    /// </summary>
    protected readonly ITPaaSApplicationAuthentication authn;

    /// <summary>
    /// Gets the custom customHeaders for the request.
    /// </summary>
    /// <remarks>
    /// Following #83476 we are deliberately passing the x-jwt-assertion header on all requests regardless of whether they're 
    /// 'internal' or not.
    /// </remarks>
    protected IDictionary<string, string> customHeaders => Request.Headers.GetCustomHeaders();
    //protected IDictionary<string, string> customHeaders => Request.Headers.GetCustomHeaders(true); //use this when debugging locally and calling other 3dpm services 

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
    /// Default constructor.
    /// </summary>
    protected BaseController(ILoggerFactory loggerFactory, IConfigurationStore configStore,
      IServiceExceptionHandler serviceExceptionHandler, IKafka producer,
      IRaptorProxy raptorProxy, IProjectRepository projectRepo, 
      ISubscriptionRepository subscriptionRepo = null, IFileRepository fileRepo = null, 
      IDataOceanClient dataOceanClient = null, ITPaaSApplicationAuthentication authn = null)
    {
      this.loggerFactory = loggerFactory;
      this.logger = loggerFactory.CreateLogger(GetType());

      this.configStore = configStore;
      this.serviceExceptionHandler = serviceExceptionHandler;
      this.producer = producer;

      if (!this.producer.IsInitializedProducer)
      {
        this.producer.InitProducer(configStore);
      }

      kafkaTopicName = (configStore.GetValueString("PROJECTSERVICE_KAFKA_TOPIC_NAME") +
                        configStore.GetValueString("KAFKA_TOPIC_NAME_SUFFIX")).Trim();

      this.projectRepo = projectRepo;
      this.subscriptionRepo = subscriptionRepo;
      this.fileRepo = fileRepo;
      this.raptorProxy = raptorProxy;
      this.dataOceanClient = dataOceanClient;
      this.authn = authn;
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
        if (logger.IsTraceEnabled())
          logger.LogTrace($"Executed {action.GetMethodInfo().Name} with result {JsonConvert.SerializeObject(result)}");
      }
      catch (ServiceException se)
      {
        logger.LogError(se, $"Execution failed for: {action.GetMethodInfo().Name}. ");
        throw;
      }
      catch (Exception ex)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError,
          ContractExecutionStatesEnum.InternalProcessingError - customErrorMessageOffset, ex.Message, innerException: ex );
      }
      finally
      {
        logger.LogInformation($"Executed {action.GetMethodInfo().Name} with the result {result?.Code}");
      }

      return result;
    }

    /// <summary>
    /// Gets the customer uid from the context.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Incorrect customer uid value.</exception>
    protected string GetCustomerUid()
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
        return principal.UserEmail;
      }

      throw new ArgumentException("Incorrect user email address in request context principal.");
    }

    /// <summary>
    /// Gets the project.
    /// </summary>
    /// <param name="legacyProjectId"></param>
    protected async Task<ProjectDatabaseModel> GetProject(long legacyProjectId)
    {
      var customerUid = LogCustomerDetails("GetProject by legacyProjectId", legacyProjectId);
      var project =
        (await projectRepo.GetProjectsForCustomer(customerUid).ConfigureAwait(false)).FirstOrDefault(
          p => p.LegacyProjectID == legacyProjectId);

      if (project == null)
      {
        logger.LogWarning($"User doesn't have access to legacyProjectId: {legacyProjectId}");
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.Forbidden, 1);
      }

      logger.LogInformation($"Project legacyProjectId: {legacyProjectId} retrieved");
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
      logger.LogInformation(
        $"{functionName}: UserUID={userId}, CustomerUID={customerUid}  and projectUid={projectUid}");

      return customerUid;
    }

    /// <summary>
    /// Log the Customer and Project details.
    /// </summary>
    /// <param name="functionName">Calling function name</param>
    /// <param name="legacyProjectId">The Project Id from legacy</param>
    /// <returns>Returns <see cref="TIDCustomPrincipal.CustomerUid"/></returns>
    protected string LogCustomerDetails(string functionName, long legacyProjectId = 0)
    {
      logger.LogInformation(
        $"{functionName}: UserUID={userId}, CustomerUID={customerUid}  and legacyProjectId={legacyProjectId}");

      return customerUid;
    }
  }
}
