﻿using VSS.Productivity3D.Scheduler.Models;

namespace VSS.Productivity3D.Scheduler.Abstractions
{
  public interface IJobRunner
  {
    string QueueHangfireJob(JobRequest request);
  }
}