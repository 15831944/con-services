﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TagFileHarvester.Interfaces
{
  public interface IHarvesterTasks
  {
    Task<T> StartNewLimitedConcurrency<T>(Func<T>action, CancellationToken token);
    Task StartNewLimitedConcurrency(Action action, CancellationToken token);
    Task<T> StartNewLimitedConcurrency2<T>(Func<T> action, CancellationToken token);
    Task StartNewLimitedConcurrency2(Action action, CancellationToken token);
    Tuple<int, int> Status();

  }
}
