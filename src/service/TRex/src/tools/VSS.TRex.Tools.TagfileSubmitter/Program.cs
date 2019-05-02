﻿using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.TRex.DI;
using Tests.Common;
using VSS.ConfigurationStore;
using VSS.TRex.Common.Utilities;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.TAGFiles.GridFabric.Arguments;
using VSS.TRex.TAGFiles.GridFabric.Requests;
using VSS.TRex.TAGFiles.Servers.Client;

/*
Arguments for building project #5, Dimensions:
5 "J:\PP\Construction\Office software\SiteVision Office\Test Files\VisionLink Data\Dimensions 2012\Dimensions2012-Model 381\Model 381"

Arguments for building project #6, Christchurch Southern Motorway:
6 "J:\PP\Construction\Office software\SiteVision Office\Test Files\VisionLink Data\Southern Motorway\TAYLORS COMP"
*/

namespace VSS.TRex.Tools.TagfileSubmitter
{
  public class TAGFileNameComparer : IComparer<string>
  {
    public int Compare(string x, string y)
    {
      // Sort the filename using the date encoded into the filename
      return x.Split('-')[4].CompareTo(y.Split('-')[4]);
    }
  }

  public class Processor
  {
    private static ILogger Log = Logging.Logger.CreateLogger<Program>();

    // Singleton request object for submitting TAG files. Creating these is relatively slow and support concurrent operations.
    private SubmitTAGFileRequest submitTAGFileRequest;
    private ProcessTAGFileRequest processTAGFileRequest;

    private int tAGFileCount = 0;

    public Guid[] ExtraProjectGuids = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

    public Guid AssetOverride = Guid.Empty;


    public void SubmitSingleTAGFile(Guid projectID, Guid assetID, string fileName)
    {
      submitTAGFileRequest = submitTAGFileRequest ?? new SubmitTAGFileRequest();
      SubmitTAGFileRequestArgument arg;

      using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
      {
        byte[] bytes = new byte[fs.Length];
        fs.Read(bytes, 0, bytes.Length);

        arg = new SubmitTAGFileRequestArgument()
        {
          ProjectID = projectID,
          AssetID = assetID,
          TagFileContent = bytes,
          TAGFileName = Path.GetFileName(fileName)
        };
      }

      Log.LogInformation($"Submitting TAG file #{++tAGFileCount}: {fileName}");

      submitTAGFileRequest.Execute(arg);
    }

    public void ProcessSingleTAGFile(Guid projectID, string fileName)
    {
      //   Machine machine = new Machine(null, "TestName", "TestHardwareID", 0, 0, Guid.NewGuid(), 0, false);
      Guid machineID = AssetOverride == Guid.Empty ? Guid.NewGuid() : AssetOverride;

      processTAGFileRequest = processTAGFileRequest ?? new ProcessTAGFileRequest();
      ProcessTAGFileRequestArgument arg;

      using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
      {
        byte[] bytes = new byte[fs.Length];
        fs.Read(bytes, 0, bytes.Length);

        arg = new ProcessTAGFileRequestArgument()
        {
          ProjectID = projectID,
          AssetUID = machineID,
          TAGFiles = new List<ProcessTAGFileRequestFileItem>()
          {
            new ProcessTAGFileRequestFileItem()
            {
              FileName = Path.GetFileName(fileName),
                            TagFileContent = bytes,
                            IsJohnDoe = false
            }
          }
        };
      }

      processTAGFileRequest.Execute(arg);
    }

    public void ProcessTAGFiles(Guid projectID, string[] files)
    {
      // Machine machine = new Machine(null, "TestName", "TestHardwareID", 0, 0, Guid.NewGuid(), 0, false);
      Guid machineID = AssetOverride == Guid.Empty ? Guid.NewGuid() : AssetOverride;

      processTAGFileRequest = processTAGFileRequest ?? new ProcessTAGFileRequest();
      ProcessTAGFileRequestArgument arg = new ProcessTAGFileRequestArgument
      {
        ProjectID = projectID,
        AssetUID = machineID,
        TAGFiles = new List<ProcessTAGFileRequestFileItem>()
      };

      foreach (string file in files)
      {
        using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
        {
          byte[] bytes = new byte[fs.Length];
          fs.Read(bytes, 0, bytes.Length);

          arg.TAGFiles.Add(new ProcessTAGFileRequestFileItem { FileName = Path.GetFileName(file), TagFileContent = bytes, IsJohnDoe = false });
        }
      }

      processTAGFileRequest.Execute(arg);
    }

    public void SubmitTAGFiles(Guid projectID, string[] files)
    {
      //   Machine machine = new Machine(null, "TestName", "TestHardwareID", 0, 0, Guid.NewGuid(), 0, false);
      Guid machineID = AssetOverride == Guid.Empty ? Guid.NewGuid() : AssetOverride;
      foreach (string file in files)
        SubmitSingleTAGFile(projectID, machineID, file);
    }

    public void CollectTAGFilesInFolder(string folder, List<string> fileNamesFromFolders)
    {
      // If it is a single file, just include it

      if (File.Exists(folder))
      {
        fileNamesFromFolders.Add(folder);
      }
      else
      {
        foreach (string f in Directory.GetDirectories(folder))
          CollectTAGFilesInFolder(f, fileNamesFromFolders);

        fileNamesFromFolders.AddRange(Directory.GetFiles(folder, "*.tag"));
      }
    }

    public void ProcessSortedTAGFilesInFolder(Guid projectID, string folder)
    {
      var fileNamesFromFolders = new List<string>();
      CollectTAGFilesInFolder(folder, fileNamesFromFolders);

      fileNamesFromFolders.Sort(new TAGFileNameComparer());

      SubmitTAGFiles(projectID, fileNamesFromFolders.ToArray());
    }

    public void ProcessTAGFilesInFolder(Guid projectID, string folder)
    {
      // If it is a single file, just process it
      if (File.Exists(folder))
      {
        // ProcessTAGFiles(projectID, new string[] { folder });
        SubmitTAGFiles(projectID, new[] { folder });
      }
      else
      {
        string[] folders = Directory.GetDirectories(folder);
        foreach (string f in folders)
        {
          ProcessTAGFilesInFolder(projectID, f);
        }

        // ProcessTAGFiles(projectID, Directory.GetFiles(folder, "*.tag"));
        SubmitTAGFiles(projectID, Directory.GetFiles(folder, "*.tag"));
      }
    }

    public void ProcessMachine333TAGFiles(Guid projectID)
    {
      ProcessSortedTAGFilesInFolder(projectID, TestCommonConsts.TestDataFilePath() + "TAGFiles\\Machine333");
    }

    public void ProcessMachine10101TAGFiles(Guid projectID)
    {
      ProcessSortedTAGFilesInFolder(projectID, TestCommonConsts.TestDataFilePath() + "TAGFiles\\Machine10101");
    }
  }

  public class Program
  {
    private static ILogger Log = Logging.Logger.CreateLogger<Program>();
    
    private static void DependencyInjection()
    {
      DIBuilder.New()
        .AddLogging()
        .Add(x => x.AddSingleton<IConfigurationStore, GenericConfiguration>())
        .Add(TRexGridFactory.AddGridFactoriesToDI)
        .Build()
        .Add(x => x.AddSingleton(new TAGFileProcessingClientServer()))
        .Complete();
    }

    public static void Main(string[] args)
    {
      DependencyInjection();

      var processor = new Processor();

      // Make sure all our assemblies are loaded...
      AssembliesHelper.LoadAllAssembliesForExecutingContext();

      Log.LogInformation("Initialising TAG file processor");

      try
      {
        // Pull relevant arguments off the command line
        if (args.Length < 2)
        {
          Console.WriteLine("Usage: ProcessTAGFiles <ProjectUID> <FolderPath>");
          Console.ReadKey();
          return;
        }

        Guid projectID = Guid.Empty;
        string folderPath;
        try
        {
          projectID = Guid.Parse(args[0]);
          folderPath = args[1];
        }
        catch
        {
          Console.WriteLine($"Invalid project ID {args[0]} or folder path {args[1]}");
          Console.ReadKey();
          return;
        }

        if (projectID == Guid.Empty)
        {
          return;
        }

        try
        {
          if (args.Length > 2)
            processor.AssetOverride = Guid.Parse(args[2]);
        }
        catch
        {
          Console.WriteLine($"Invalid Asset ID {args[2]}");
          return;
        }

        try
        {
          processor.ProcessSortedTAGFilesInFolder(projectID, folderPath);
        }
        catch (Exception e)
        {
          Console.WriteLine($"Exception: {e}");
        }

        // ProcessMachine10101TAGFiles(projectID);
        // ProcessMachine333TAGFiles(projectID);

        //ProcessSingleTAGFile(projectID, TestConsts.TestDataFilePath() + "TAGFiles\\Machine10101\\2085J063SV--C01 XG 01 YANG--160804061209.tag");
        //ProcessSingleTAGFile(projectID);

        // Process all TAG files for project 4733:
        //ProcessTAGFilesInFolder(projectID, TestConsts.TestDataFilePath() + "TAGFiles\\Model 4733\\Machine 1");
        //ProcessTAGFilesInFolder(projectID, TestConsts.TestDataFilePath() + "TAGFiles\\Model 4733\\Machine 2");
        //ProcessTAGFilesInFolder(projectID, TestConsts.TestDataFilePath() + "TAGFiles\\Model 4733\\Machine 3");
        //ProcessTAGFilesInFolder(projectID, TestConsts.TestDataFilePath() + "TAGFiles\\Model 4733\\Machine 4");
      }
      finally
      {
        Console.WriteLine("TAG file submission complete. Press a key...");

        Console.ReadKey();
      }
    }
  }
}
