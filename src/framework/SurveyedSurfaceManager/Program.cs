﻿using System;
using System.Windows.Forms;
using VSS.TRex.Common.Utilities;
using VSS.TRex.DI;
using VSS.TRex.Servers.Client;

namespace SurveyedSurfaceManager
{
  static class Program
  {

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    private static void DependencyInjection()
    {
      DIBuilder.New().AddLogging().Complete();
    }

    [STAThread]
    static void Main()
    {
      DependencyInjection();

      // Make sure all our assemblies are loaded...
      AssembliesHelper.LoadAllAssembliesForExecutingContext();

      ImmutableClientServer server = new ImmutableClientServer("SurveyedSurfaceManager");

      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Application.Run(new Form1());
    }
  }
}
