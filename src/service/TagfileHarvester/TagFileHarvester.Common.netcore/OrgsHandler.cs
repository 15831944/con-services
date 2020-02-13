﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TagFileHarvester.Interfaces;
using TagFileHarvester.Models;
using TagFileHarvester.TaskQueues;

namespace TagFileHarvester
{
  public static class OrgsHandler
  {
    //Static settings initialized here
    public static int MaxThreadsToProcessTagFiles = 256;
    public static string tccSynchFilespaceShortName;
    public static string tccSynchMachineFolder;
    public static string TCCSynchProductionDataFolder;
    public static string TCCSynchProductionDataArchivedFolder;
    public static string TCCSynchProjectBoundaryIssueFolder;
    public static string TCCSynchSubscriptionIssueFolder;
    public static string TCCSynchOtherIssueFolder;
    public static TimeSpan TagFileSubmitterTasksTimeout;
    public static TimeSpan TCCRequestTimeout;
    public static int NumberOfFilesInPackage;
    public static TimeSpan OrgProcessingDelay;
    public static byte TagFilesFolderLifeSpanInDays;
    public static string ShortOrgName;
    public static string newrelic = "";

    private static ILogger log;

    public static readonly object OrgListLocker = new object();

    public static Dictionary<Task, Tuple<Organization, CancellationTokenSource>> OrgProcessingTasks { get; } =
      new Dictionary<Task, Tuple<Organization, CancellationTokenSource>>();

    public static IUnityContainer Container { get; private set; }
    public static bool CacheEnabled { get; set; }
    public static bool FilenameDumpEnabled { get; set; }
    public static string TagFileEndpoint { get; set; }

    public static string VssServiceName { get; set; }


    public static void ForEach<T>(
      this IEnumerable<T> source,
      Action<T> action)
    {
      foreach (var element in source)
        action(element);
    }

    public static void Clean()
    {
      OrgProcessingTasks.Clear();
    }

    public static void Initialize(IUnityContainer container)
    {
      Container = container;
      log = Container.Resolve<ILogger>();
    }

    public static List<Organization> GetOrgs()
    {
      return Container.Resolve<IFileRepository>().ListOrganizations();
    }

    public static void CheckAvailableOrgs(object sender)
    {
      try
      {
        //Make sure that data export is running
        var orgs = GetOrgs();
        log.LogInformation("Got {0} orgs from repository", orgs.Count);
        MergeAndProcessOrgs(orgs);
      }
      catch (Exception ex)
      {
        log.LogError("Exception while listing orgs occured {0}", ex.Message);
      }
    }

    private static void MergeAndProcessOrgs(List<Organization> orgs)
    {
      var result = "";
      foreach (var organization in orgs)
        result += organization.orgDisplayName + "," + organization.shortName + "," + organization.filespaceId + "," +
                  organization.orgId + "\n";
      //Filter out all stopped\completed tasks
      lock (OrgListLocker)
      {
        OrgProcessingTasks.Where(o => o.Key.IsCompleted || o.Key.IsFaulted || o.Key.IsCanceled)
          .Select(d => d.Key)
          .ForEach(d => OrgProcessingTasks.Remove(d));
        log.LogInformation("Currently processing orgs: {0} ",
          OrgProcessingTasks.Select(o => o.Value.Item1).DefaultIfEmpty(new Organization())
            .Select(t => t.shortName).Aggregate((current, next) => current + ", " + next));
        log.LogDebug("Tasks status when trying to add new orgs is {0} in Queue1 and {1} in Queue2 on {2} Threads",
          Container.Resolve<IHarvesterTasks>().Status().Item1,
          Container.Resolve<IHarvesterTasks>().Status().Item2, GetUsedThreads());
        //do merge here - if there is no org in the list of tasks - build it. If there is no org but there in the list but there is a task - kill the task
        orgs.Where(o => !OrgProcessingTasks.Select(t => t.Value.Item1).Contains(o))
          .Where(o => string.IsNullOrEmpty(ShortOrgName) || o.shortName == ShortOrgName).ForEach(o =>
          {
            log.LogInformation("Adding {0} org for processing", o.shortName);
            var cancellationToken = new CancellationTokenSource();
            OrgProcessingTasks.Add(Container.Resolve<IHarvesterTasks>().StartNewLimitedConcurrency2(() =>
            {
              new OrgProcessorTask(Container, o, cancellationToken, OrgProcessingTasks).ProcessOrg(false,
                t => log
                  .LogInformation("Tasks status is {0} in Queue1 and {1} in Queue2 on {2} Threads",
                    Container.Resolve<IHarvesterTasks>().Status().Item1,
                    Container.Resolve<IHarvesterTasks>().Status().Item2, GetUsedThreads()));
            }, cancellationToken.Token, false), new Tuple<Organization, CancellationTokenSource>(o, cancellationToken));
          });
        //Reversed situation - org has been removed from filespaces but there is a task - cancel it
        if (OrgProcessingTasks.Any(o => !orgs.Contains(o.Value.Item1)))
        {
          OrgProcessingTasks.Where(o => !orgs.Contains(o.Value.Item1)).ForEach(o => o.Value.Item2.Cancel());
          log.LogInformation("Removing {0} org from processing",
            OrgProcessingTasks.Select(o => o.Value.Item1)
              .Where(y => !orgs.Contains(y))
              .Select(t => t.shortName)
              .Aggregate((current, next) => current + ", " + next));
          OrgProcessingTasks.Where(o => !orgs.Contains(o.Value.Item1))
            .Select(d => d.Key)
            .ForEach(t => OrgProcessingTasks.Remove(t));
        }
      }
    }

    public static int GetUsedThreads()
    {
      ThreadPool.GetAvailableThreads(out var i, out _);
      ThreadPool.GetMaxThreads(out var j, out _);
      return j - i;
    }
  }
}
