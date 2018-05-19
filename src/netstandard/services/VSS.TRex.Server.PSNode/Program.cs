﻿using System;
using VSS.TRex.DI;
using VSS.TRex.Servers.Compute;

namespace VSS.TRex.Server.PSNode
{
  class Program
  {
    private static void DependencyInjection()
    {
      DIContext.Inject(DIImplementation.New().ConfigureLogging().Build());
    }

    static void Main(string[] args)
    { 
      DependencyInjection();

      var server = new SubGridProcessingServer();
      Console.WriteLine("Press anykey to exit");
      Console.ReadLine();
    }
  }
}
