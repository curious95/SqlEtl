// <copyright file="Program.cs" company="">
// Copyright (c) 2016 All Right Reserved,
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// </copyright>
// <author>Udaiappa Ramachandran</author>
// <email>udaiappa@gmail.com</email>
// <date>2016-01-15</date>
// <summary></summary>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SqlEtl.Entities;
using SqlEtl.Helpers;
using SqlEtl.Logging;
using SqlEtl.Resource;

namespace SqlEtl
{
    /// <summary>
    ///     Utility to upload data using bcp
    /// </summary>
    internal class Program
    {
        private static readonly BulkCopyRequest Request = new BulkCopyRequest();

        private static void Main(string[] args)
        {
            Console.Title = Message.Program_Main_Title;
            var c = Console.ForegroundColor;
            try
            {
                if (!ParseArgs(args))
                {
                    Arguments.PrintHelp();
                    Console.Read();
                    return;
                }

                var bcm = new BulkCopyManager();
                bcm.BulkCopyProgress += bcm_BulkCopyProgress;
                var response = bcm.BulkCopy(Request);
                Log("--------------------------------------------------");
                Log("Start Time:" + response.StartTime);
                Log("End Time:" + response.EndTime);
                Log("Total Changes:" + response.ChangesTotal);
                Log("Total Applied:" + response.ChangesApplied);
                Log("Total Failed:" + response.ChangesFailed);
                Log("--------------------------------------------------");
                if (response.Status != null && response.Status.Count > 0)
                {
                    Log("The following heaps are skipped");
                    Dictionary<string, string> skip;
                    if (response.Status.TryGetValue("Skipped", out skip))
                    {
                        foreach (var item in skip)
                        {
                            Log(item.Key + "--" + item.Value);
                        }
                    }
                }
                Log("--------------------------------------------------");
                Log("Bulk copy completed.");
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Log("Bulk copy failed.");
                Log(e.ToString());
            }
            Console.ForegroundColor = c;
            Log("Please press [Enter] to exit.");
            Console.ReadLine();
        }

        private static void bcm_BulkCopyProgress(object sender, BulkCopyProgressEventArgs e)
        {
            Log(e.Result);
        }

        private static void Log(string msg)
        {
            Logger.Log(LogLevels.Info, msg);
            Console.WriteLine(msg);
        }

        private static bool ParseArgs(string[] args)
        {
            try
            {
                var commandLine = new Arguments(args);

                if (!string.IsNullOrWhiteSpace(commandLine["Source"]))
                {
                    Request.LocalConnectionString = commandLine["Source"];
                }
                else
                {
                    return false;
                }
                if (!string.IsNullOrWhiteSpace(commandLine["Destination"]))
                {
                    Request.RemoteConnectionString = commandLine["Destination"];
                }
                else
                {
                    return false;
                }
                if (!string.IsNullOrWhiteSpace(commandLine["ResumeOnError"]))
                {
                    Request.ResumeOnError = Convert.ToBoolean(commandLine["ResumeOnError"]);
                }
                if (!string.IsNullOrWhiteSpace(commandLine["BatchSize"]))
                {
                    Request.BatchSize = Convert.ToInt32(commandLine["BatchSize"]);
                }
                if (!string.IsNullOrWhiteSpace(commandLine["Skip"]))
                {
                    Request.Skip = commandLine["Skip"].ToLower().Replace("*", "%").Split(',');
                }
                if (!string.IsNullOrWhiteSpace(commandLine["CreateObjects"]))
                {
                    Request.CreateObjects = Convert.ToBoolean(commandLine["CreateObjects"]);
                }
                if (!string.IsNullOrWhiteSpace(commandLine["RetryCount"]))
                {
                    Request.RetryCount = Convert.ToInt32(commandLine["RetryCount"]);
                }
                if (!string.IsNullOrWhiteSpace(commandLine["RetryInterval"]))
                {
                    Request.RetryInterval = Convert.ToInt32(commandLine["RetryInterval"]);
                }
                if (!string.IsNullOrWhiteSpace(commandLine["CustomKeyDefinitions"]))
                {
                    var fileName = commandLine["CustomKeyDefinitions"];
                    if (!string.IsNullOrWhiteSpace(fileName) && File.Exists(fileName))
                    {
                        var lines = File.ReadAllLines(fileName);
                        foreach (var parts in lines.Select(line => line.Split('~')).Where(parts => parts.Length == 2))
                        {
                            Request.CustomKeyDefinitions.Add(parts[0], parts[1].Split(','));
                        }
                    }
                }

            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}