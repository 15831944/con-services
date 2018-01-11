﻿using System;
using System.Collections.Generic;
using Hangfire.Server;

namespace VSS.Productivity3D.Scheduler.WebAPI.ExportJobs
{
  public interface IVetaExportJob
  {
    /// <summary>
    /// Exports to Veta.
    /// </summary>
    void ExportDataToVeta(Guid projectUid, string fileName, string machineNames, Guid? filterUid,
      IDictionary<string, string> customHeaders,
      PerformContext context);
  }
}