﻿using System;
using System.Windows.Forms;
using VSS.TRex.DI;

namespace VSS.TRex.IgnitePOC.TestApp
{
  static class Program
  {
    private static void DependencyInjection()
    {
      DIContext.Inject(DIImplementation.New().ConfigureLogging().Build());
    }

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
      DependencyInjection();

      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Application.Run(new Form1());
    }
  }
}
