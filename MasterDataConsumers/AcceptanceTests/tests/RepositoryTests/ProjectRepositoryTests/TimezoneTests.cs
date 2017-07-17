﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using RepositoryTests.Internal;
using VSS.MasterData.Repositories;

namespace RepositoryTests.ProjectRepositoryTests
{
  [TestClass]
  public class TimezoneTests
  {
    /// <summary>
    /// These Timezone conversions need to be done as acceptance tests so they are run on linux - the target platform.
    /// They should behave ok on windows (this will occur if you run a/ts against local containers).
    /// </summary>
    [TestMethod]
    public void ConvertTimezone_WindowsToIana()
    {
      Assert.AreEqual("Pacific/Auckland", TimeZone.WindowsToIana(ProjectTimezones.NewZealandStandardTime), "Unable to convert WindowsToIana");
    }

    [TestMethod]
    public void ConvertTimezone_WindowsToIana_Invalid()
    {
      Assert.AreEqual("", TimeZone.WindowsToIana("New Zealand Standard Time222"), "Should not be able to convert WindowsToIana");
    }

    [TestMethod]
    public void ConvertTimezone_WindowsToIana_alreadyIana()
    {
      Assert.AreEqual("", TimeZone.WindowsToIana(ProjectTimezones.PacificAuckland), "Should not be able to convert WindowsToIana");
    }

    [TestMethod]
    public void ConvertTimezone_WindowsToIana_UTC()
    {
      Assert.AreEqual("Etc/UTC", TimeZone.WindowsToIana(ProjectTimezones.Utc), "Unable to convert WindowsToIana");
    }
  }
}