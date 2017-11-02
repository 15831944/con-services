﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using Dapper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using VSS.Productivity3D.Scheduler.Common.Utilities;

namespace SchedulerTests
{
  [TestClass]
  public class FilterSchedulerTests : TestControllerBase
  {
    protected ILogger _log;
    string FilterCleanupTask = "FilterCleanupTask";
    private int _bufferForDBUpdateMs = 1000;

    [TestInitialize]
    public void Init()
    {
      SetupDi();

      _log = LoggerFactory.CreateLogger<FilterSchedulerTests>();
      Assert.IsNotNull(_log, "Log is null");
    }

    [TestMethod]
    public void FilterScheduleTaskExists()
    {
      var theJob = GetJob(HangfireConnection(), FilterCleanupTask);
    
      Assert.IsNotNull(theJob,$"{FilterCleanupTask} not found");
      Assert.AreEqual(FilterCleanupTask, theJob.Id, "wrong job selected");
    }

    [TestMethod]
    public void FilterScheduleTaskNextRuntime()
    {
      var theJob = GetJob(HangfireConnection(), FilterCleanupTask);

      Assert.IsNotNull(theJob, $"{FilterCleanupTask} not found");
      Assert.AreEqual(FilterCleanupTask, theJob.Id, "wrong job selected");
      Assert.IsNotNull(theJob.NextExecution, $"{FilterCleanupTask} nextExecutionTime not found");
      Assert.IsTrue(theJob.NextExecution < DateTime.UtcNow.AddMinutes(1).AddSeconds(1), $"{FilterCleanupTask} nextExecutionTime not within one minute");
    }

    [TestMethod]
    public void FilterScheduleTask_WaitForCleanup()
    {
      var theJob = GetJob(HangfireConnection(), FilterCleanupTask);

      string filterDbConnectionString = ConnectionUtils.GetConnectionString(ConfigStore, _log, "_FILTER");
      var dbConnection = new MySqlConnection(filterDbConnectionString);
      dbConnection.Open();

      //var filter = new Filter(){}; //todo?
      var filterUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid().ToString();
      var projectUid = Guid.NewGuid().ToString();
      var userUid = Guid.NewGuid().ToString();
      var name = "";
      var filterJson = "";
      var actionUtc = new DateTime(2017, 1, 1); // eventObject.EventDate:yyyy-MM-dd HH\:mm\:ss.fffffff
      var empty = "\"";
      string insertFilter = string.Format(
        $"INSERT Filter (FilterUID, fk_CustomerUid, fk_ProjectUID, UserID, Name, FilterJson, LastActionedUTC) " +
        $"VALUES ({empty}{filterUid}{empty}, { empty}{customerUid}{empty}, {empty}{projectUid}{empty}, {empty}{userUid}{empty}, " +
        $"{empty}{filterJson}{empty}, {empty}{name}{empty}, {empty}{actionUtc.ToString($"yyyy-MM-dd HH:mm:ss.fffffff")}{empty})");

      int insertedCount = 0;
      insertedCount = dbConnection.Execute(insertFilter);
      Assert.AreEqual(1,insertedCount,"Filter Not Inserted");
      Assert.IsNotNull(theJob.NextExecution, "theJob.NextExecution != null");
      
      string selectFilter = string.Format($"SELECT FilterUID FROM Filter WHERE FilterUID = {empty}{filterUid}{empty}");
      Console.WriteLine($"FilterScheduleTask_WaitForCleanup: connectionString {dbConnection.ConnectionString} selectFilter {selectFilter} insertFilter {insertFilter}");
      IEnumerable<object> response = null;
      for (int i = 0; i < 10; i++)
      {
        // seems to be a bit of a delay when dealing with NextExecution datetime.
        // It's not very accurate and NextExecution doesn't get updated in a timely fashion after executed.
        // Also, doesn't seem to pick up the insert quickly - is the delay in the Insert or scheduler, I don't know...
        var nextExec = theJob.NextExecution.Value;
        var nowUtc = DateTime.UtcNow;
        var msToWait = (int)(nextExec - nowUtc).TotalMilliseconds + _bufferForDBUpdateMs;
        if (msToWait > 0)
          Thread.Sleep(msToWait);
        else
          Thread.Sleep(10000);

        response = dbConnection.Query(selectFilter);
        Console.WriteLine($"FilterScheduleTask_WaitForCleanup: iteration {i} nextExec {nextExec} nowUTC {nowUtc} _bufferForDBUpdateMs {_bufferForDBUpdateMs} msToWait {msToWait} response {JsonConvert.SerializeObject(response)}");
        
        if (response != null && !response.Any() )
          break;
      }
      
      Assert.IsNotNull(response, "Should have a response.");
      Assert.AreEqual(0, response.Count(), "No filters should be returned.");

      dbConnection.Close();
    }
  }
}
