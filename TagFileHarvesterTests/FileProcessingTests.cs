﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TagFileHarvester;
using TagFileHarvester.Implementation;
using TagFileHarvester.Interfaces;
using TagFileHarvester.Models;
using TagFileHarvester.TaskQueues;
using TagFileHarvesterTests.Mock;
using TagFileHarvesterTests.MockRepositories;

namespace TagFileHarvesterTests
{
  [TestClass]
  public class FileProcessingTests
  {

    private static IUnityContainer unityContainer;
    private readonly MockFileRepository respositoryInstance = new MockFileRepository();
    private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType);

    [TestInitialize]
    public void Initialize()
    {
      unityContainer = new UnityContainer();
      unityContainer.RegisterInstance<IFileRepository>(respositoryInstance);
      unityContainer.RegisterType<IHarvesterTasks, MockTaskScheduler>();
      unityContainer.RegisterInstance<ILog>(log);
      unityContainer.RegisterInstance<IBookmarkManager>(XMLBookMarkManager.Instance);
      OrgsHandler.Clean();
      respositoryInstance.Clean();
      OrgsHandler.Initialize(unityContainer);
    }

    [TestCleanup]
    public void TestCleanup()
    {
      XMLBookMarkManager.Instance.DeleteFile();
      XMLBookMarkManager.Instance.DeleteMergeFile();
      XMLBookMarkManager.Instance.DeleteInstance();
      unityContainer.RemoveAllExtensions();
      
     }

    [TestMethod]
    public void CanRemoveAbsentOrgs()
    {
      OrgsHandler.CheckAvailableOrgs(null);
      //Save here two items from the list of created tasks
    //  var org1 = OrgsHandler.OrgProcessingTasks[2];
      //var org2 = OrgsHandler.OrgProcessingTasks[5];
      //Remove these orgs from the list of orgs
      //respositoryInstance.DeleteOrgs(new Organization[]{org1.Item1, org2.Item1});
      OrgsHandler.CheckAvailableOrgs(null);
      Assert.AreEqual(MockFileRepository.OrgIncrement * 2 - 2, OrgsHandler.OrgProcessingTasks.Count);
    }

    [TestMethod]
    public void CanAddMoreOrgsToExisting()
    {
      OrgsHandler.CheckAvailableOrgs(null);
      //Add here more orgs here
      //And make sure that they are merged
      OrgsHandler.CheckAvailableOrgs(null);
      Assert.AreEqual(MockFileRepository.OrgIncrement*2,OrgsHandler.OrgProcessingTasks.Count);

    }

    [TestMethod]
    public void CanAddNewlyFoundOrgs()
    {
      OrgsHandler.CheckAvailableOrgs(null);
      Assert.AreEqual(MockFileRepository.OrgIncrement, OrgsHandler.OrgProcessingTasks.Count);
    }

    [TestMethod]
    public void CanListOrgs()
    {
      Assert.AreEqual(MockFileRepository.OrgIncrement, OrgsHandler.GetOrgs().Count);
    }
  }
}
