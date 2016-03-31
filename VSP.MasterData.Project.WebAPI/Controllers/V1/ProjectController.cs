﻿using System;
using System.Configuration;
using System.Reflection;
using System.Web.Http;
using log4net;
using org.apache.kafka.clients.producer;
using VSP.MasterData.Project.WebAPI.Helpers;
using VSP.MasterData.Common.Logging;
using VSS.Kafka.DotNetClient.Interfaces;
using VSS.Kafka.DotNetClient.Model;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;


namespace VSP.MasterData.Project.WebAPI.Controllers.V1
{
  [RoutePrefix("v1")]
  public class ProjectV1Controller : ApiController
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly IProducer _producer;

    public ProjectV1Controller(IProducer producer)
    {
      _producer = producer;
    }

    // POST: api/project
    /// <summary>
    /// Create Project
    /// </summary>
    /// <param name="project">CreateProjectEvent model</param>
    /// <remarks>Create new project</remarks>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad request</response>
    [Route("")]
    [HttpPost]
    public IHttpActionResult CreateProject([FromBody] CreateProjectEvent project)
    {
      try
      {
        project.ReceivedUTC = DateTime.UtcNow;
        var jsonHelper = new JsonHelper();
        var messagePayload = jsonHelper.SerializeObjectToJson(new { CreateProjectEvent = project });
        var message = new Message { Key = project.ProjectUID.ToString(), Value = messagePayload };

        var producerRecord = new ProducerRecord(ConfigurationManager.AppSettings["KafkaTopicName"], message);
        _producer.send(producerRecord).get();

        return Ok();
        throw new Exception("Failed to publish message to Kafka");
      }
      catch (Exception ex)
      {
        Log.IfError(ex.Message + ex.StackTrace);
        return InternalServerError();
      }
    }

    // PUT: api/Project
    /// <summary>
    /// Update Project
    /// </summary>
    /// <param name="project">UpdateProjectEvent model</param>
    /// <remarks>Updates existing project</remarks>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad request</response>
    [Route("")]
    [HttpPut]
    public IHttpActionResult UpdateProject([FromBody] UpdateProjectEvent project)
    {
      try
      {
        var jsonHelper = new JsonHelper();
        project.ReceivedUTC = DateTime.UtcNow;
        var messagePayload = jsonHelper.SerializeObjectToJson(new { UpdateProjectEvent = project });
        var message = new Message { Key = project.ProjectUID.ToString(), Value = messagePayload };

        var producerRecord = new ProducerRecord(ConfigurationManager.AppSettings["KafkaTopicName"], message);
        _producer.send(producerRecord).get();

          return Ok();
        throw new Exception("Failed to publish message to Kafka");
      }
      catch (Exception ex)
      {
        Log.IfError(ex.Message + ex.StackTrace);
        return InternalServerError();
      }
    }

    // DELETE: api/Project/
    /// <summary>
    /// Delete Project
    /// </summary>
    /// <param name="projectUID">DeleteProjectEvent model</param>
    /// <param name="userUID">DeleteProjectEvent model</param>
    /// <param name="actionUTC">DeleteProjectEvent model</param>
    /// <remarks>Deletes project with projectUID</remarks>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad request</response>

    [Route("")]
    [HttpDelete]
    public IHttpActionResult DeleteProject(Guid projectUID, DateTime actionUTC)
    {
      try
      {
        var project = new DeleteProjectEvent();
        project.ProjectUID = projectUID;
        project.ActionUTC = actionUTC;

        project.ReceivedUTC = DateTime.UtcNow;
        var jsonHelper = new JsonHelper();

        var messagePayload = jsonHelper.SerializeObjectToJson(new { DeleteProjectEvent = project });
        var message = new Message { Key = project.ProjectUID.ToString(), Value = messagePayload };
        var producerRecord = new ProducerRecord(ConfigurationManager.AppSettings["KafkaTopicName"], message);
        _producer.send(producerRecord).get();

        return Ok();
        throw new Exception("Failed to publish message to Kafka");
      }
      catch (Exception ex)
      {
        Log.IfError(ex.Message + ex.StackTrace);
        return InternalServerError();
      }
    }


    // POST: api/project
    /// <summary>
    /// Restore Project
    /// </summary>
    /// <param name="project">CreateProjectEvent model</param>
    /// <remarks>Create new project</remarks>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad request</response>
    [Route("Restore")]
    [HttpPost]
    public IHttpActionResult RestoreProject([FromBody] RestoreProjectEvent project)
    {
      try
      {
        project.ReceivedUTC = DateTime.UtcNow;
        var jsonHelper = new JsonHelper();
        var messagePayload = jsonHelper.SerializeObjectToJson(new { RestoreProjectEvent = project });
        var message = new Message { Key = project.ProjectUID.ToString(), Value = messagePayload };
        var producerRecord = new ProducerRecord(ConfigurationManager.AppSettings["KafkaTopicName"], message);
        _producer.send(producerRecord).get();

        return Ok();
        throw new Exception("Failed to publish message to Kafka");
      }
      catch (Exception ex)
      {
        Log.IfError(ex.Message + ex.StackTrace);
        return InternalServerError();
      }
    }

    /// <summary>
    /// Associate customer and project
    /// </summary>
    /// <param name="customerProject">Customer - project</param>
    /// <param name="topic">(Optional)Topic to publish on. Used for test purposes.</param>
    /// <remarks>Associate customer and asset</remarks>
    /// <response code="200">Ok</response>
    /// <response code="500">Internal Server Error</response>
    [HttpPost]
    [Route("AssociateCustomer")]
    public IHttpActionResult AssociateCustomerProject([FromBody] AssociateProjectCustomer customerProject)
    {
      try
      {
        customerProject.ReceivedUTC = DateTime.UtcNow;
        var messagePayload = new JsonHelper().SerializeObjectToJson(new { AssociateCustomerAssetEvent = customerProject });
        var message = new Message { Key = customerProject.ProjectUID.ToString(), Value = messagePayload };
        var producerRecord = new ProducerRecord(ConfigurationManager.AppSettings["KafkaTopicName"], message);
        _producer.send(producerRecord).get();
        return Ok();
        throw new Exception("Failed to publish message to Kafka");
      }
      catch (Exception ex)
      {
        Log.IfError(ex.Message + ex.StackTrace);
        return InternalServerError(ex);
      }
    }

    /// <summary>
    /// Dissociate customer and asset
    /// </summary>
    /// <param name="customerProject">Customer - Project model</param>
    /// <param name="topic">(Optional)Topic to publish on. Used for test purposes.</param>
    /// <remarks>Dissociate customer and asset</remarks>
    /// <response code="200">Ok</response>
    /// <response code="500">Internal Server Error</response>
    [HttpPost]
    [Route("DissociateCustomer")]
    public IHttpActionResult DissociateCustomerProject([FromBody] DissociateProjectCustomer customerProject)
    {
      try
      {
        customerProject.ReceivedUTC = DateTime.UtcNow;
        var messagePayload = new JsonHelper().SerializeObjectToJson(new { DissociateCustomerAssetEvent = customerProject });
        var message = new Message { Key = customerProject.ProjectUID.ToString(), Value = messagePayload };
        var producerRecord = new ProducerRecord(ConfigurationManager.AppSettings["KafkaTopicName"], message);
        _producer.send(producerRecord).get();
        return Ok();
        throw new Exception("Failed to publish message to Kafka");
      }
      catch (Exception ex)
      {
        Log.IfError(ex.Message + ex.StackTrace);
        return InternalServerError(ex);
      }
    }

  }
}
