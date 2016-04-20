﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LandfillService.AcceptanceTests.Models.Landfill;
using AutomationCore.API.Framework.Library;
using Newtonsoft.Json;
using LandfillService.AcceptanceTests.Auth;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LandfillService.AcceptanceTests.Utils
{
    public class ProjectsUtils
    {
        public static Project GetProjectDetails(string name)
        {
            string response = RestClientUtil.DoHttpRequest(Config.LandfillBaseUri, "GET", TPaaS.BearerToken,
                RestClientConfig.JsonMediaType, null, System.Net.HttpStatusCode.OK, "Bearer", null);
            List<Project> projects = JsonConvert.DeserializeObject<List<Project>>(response);
            Project project = projects.FirstOrDefault(p => p.name == name);

            Assert.IsNotNull(project, "Project not found.");
            return project;
        }
    }
}
