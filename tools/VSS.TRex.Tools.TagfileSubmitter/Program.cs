﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using log4net;
using log4net.Config;
using VSS.TRex.Machines;
using VSS.TRex.TAGFiles.GridFabric.Arguments;
using VSS.TRex.TAGFiles.GridFabric.Requests;
using VSS.TRex.TAGFiles.Servers.Client;
using VSSTests.TRex.Tests.Common;

namespace VSS.TRex.Tools.TagfileSubmitter
{
    class Program
    {
        private static ILog Log = null;
        //        private static int tAGFileCount = 0;

        public static void ProcessSingleTAGFile(Guid projectID, string fileName)
        {
            Machine machine = new Machine(null, "TestName", "TestHardwareID", 0, 0, Guid.NewGuid(), 0, false);

            ProcessTAGFileRequest request = new ProcessTAGFileRequest();
            ProcessTAGFileRequestArgument arg = null;

            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                byte[] bytes = new byte[fs.Length];
                fs.Read(bytes, 0, bytes.Length);

                arg = new ProcessTAGFileRequestArgument()
                {
                    ProjectID = projectID,
                    AssetID = machine.ID,
                    TAGFiles = new List<ProcessTAGFileRequestFileItem>()
                    {
                        new ProcessTAGFileRequestFileItem()
                        {
                            FileName = fileName,
                            TagFileContent = bytes
                        }
                    }
                };
            }

            request.Execute(arg);
        }

        public static void ProcessTAGFiles(Guid projectID, string[] files)
        {
            Machine machine = new Machine(null, "TestName", "TestHardwareID", 0, 0, Guid.NewGuid(), 0, false);

            ProcessTAGFileRequest request = new ProcessTAGFileRequest();
            ProcessTAGFileRequestArgument arg = new ProcessTAGFileRequestArgument
            {
                ProjectID = projectID,
                AssetID = machine.ID,
                TAGFiles = new List<ProcessTAGFileRequestFileItem>()
            };

            foreach (string file in files)
            {
                using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    byte[] bytes = new byte[fs.Length];
                    fs.Read(bytes, 0, bytes.Length);

                    arg.TAGFiles.Add(new ProcessTAGFileRequestFileItem() {FileName = file, TagFileContent = bytes});
                }
            }

            Console.WriteLine(string.Format("Submitting {0} tagfiles for processing", files.Length));

            request.Execute(arg);
        }

        public static void ProcessTAGFilesInFolder(Guid projectID, string folder)
        {
            // If it is a single file, just process it
            if (File.Exists(folder))
            {
                ProcessTAGFiles(projectID, new string[] {folder});
            }
            else
            {
                string[] folders = Directory.GetDirectories(folder);
                foreach (string f in folders)
                {
                    ProcessTAGFilesInFolder(projectID, f);
                }

                ProcessTAGFiles(projectID, Directory.GetFiles(folder));
            }
        }

        public static void ProcessMachine333TAGFiles(Guid projectID)
        {
            ProcessTAGFilesInFolder(projectID, TAGTestConsts.TestDataFilePath() + "TAGFiles\\Machine333");
        }

        public static void ProcessMachine10101TAGFiles(Guid projectID)
        {
            ProcessTAGFilesInFolder(projectID, TAGTestConsts.TestDataFilePath() + "TAGFiles\\Machine10101");
        }

        static void Main(string[] args)
        {
            // Initialise the Log4Net logging system
            string logFileName = System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".log";
            log4net.GlobalContext.Properties["LogName"] = logFileName;
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository);

            Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

            Log.Info("Initialising TAG file processor");

            try
            {
                // Pull relevant arguments off the command line
                if (args.Length < 2)
                {
                    Console.WriteLine("Usage: ProcessTAGFiles <ProjectID> <FolderPath>");
                    return;
                }

                Guid projectID = Guid.Empty;
                string folderPath = "";
                try
                {
                    projectID = Guid.Parse(args[0]);
                    folderPath = args[1];
                }
                catch
                {
                    Console.WriteLine(string.Format("Invalid project ID {0} or folder path {1}", args[0], args[1]));
                    return;
                }

                if (projectID == Guid.Empty)
                {
                    return;
                }

                // Obtain a TAGFileProcessing client server
                TAGFileProcessingClientServer TAGServer = new TAGFileProcessingClientServer();

                Console.WriteLine(string.Format("Submitting Tagfiles for project ID {0} or folder path {1}", projectID, folderPath));
                ProcessTAGFilesInFolder(projectID, folderPath);
                Console.WriteLine("*** Tagfile Submission Complete ***");

        // ProcessMachine10101TAGFiles(projectID);
        // ProcessMachine333TAGFiles(projectID);

        //ProcessSingleTAGFile(projectID, TAGTestConsts.TestDataFilePath() + "TAGFiles\\Machine10101\\2085J063SV--C01 XG 01 YANG--160804061209.tag");
        //ProcessSingleTAGFile(projectID);

        // Process all TAG files for project 4733:
        //ProcessTAGFilesInFolder(projectID, TAGTestConsts.TestDataFilePath() + "TAGFiles\\Model 4733\\Machine 1");
        //ProcessTAGFilesInFolder(projectID, TAGTestConsts.TestDataFilePath() + "TAGFiles\\Model 4733\\Machine 2");
        //ProcessTAGFilesInFolder(projectID, TAGTestConsts.TestDataFilePath() + "TAGFiles\\Model 4733\\Machine 3");
        //ProcessTAGFilesInFolder(projectID, TAGTestConsts.TestDataFilePath() + "TAGFiles\\Model 4733\\Machine 4");
      }
      finally
            {
                Console.ReadKey();
            }
        }
    }
}
