//#define Debug
//#define Log
#define Test

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using WorkflowVerifyer.App.Helpers;

namespace WorkflowVerifyer.App
{
    internal class Program
    {
        private static Int32 m_VerificationsProcessed;
        private static Boolean m_LogToS3;

        private static int Main(String[] a_Args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            Console.WriteLine("Starting application...");

#if !Test
            Run(a_Args);
#else
            Test();
            Console.ReadKey();
#endif

            return 0;
        }
        private static void Run(String[] a_Args)
        {
            Dictionary<String, Object> l_ArgValuePairs = ArgumentExtraction.ExtractArgValuePairs(a_Args);
            Int64 l_TimeInterval = Convert.ToInt64(l_ArgValuePairs[nameof(ArgumentKey.TimeInterval)]);
            String l_ClientArgumentString = (l_ArgValuePairs[nameof(ArgumentKey.Client)]).ToString();
            List<Int32> l_Clients = ArgumentExtraction.ReturnClients(l_ClientArgumentString);
            Int32 l_Delay = Convert.ToInt32(l_ArgValuePairs[nameof(ArgumentKey.Delay)]);
            String l_TempDirPath = ConfigurationManager.AppSettings["TempFileDirectory"];
            m_LogToS3 = l_ArgValuePairs[nameof(ArgumentKey.LogToS3)].ToString() == "1";

#if (Log)
            String logInfo =
                $"LastRunTime: {l_LastRunTime}\n" +
                $"TimeInterval: {l_TimeInterval}\n" +
                $"Client: {l_ClientArgumentString}\n" +
                $"Delay: {l_Delay}\n" +
                $"LogToS3: {m_LogToS3}";
            Console.WriteLine(logInfo);

            String clientInfo = "Clients Extracted: ";
            foreach (Int32 clientID in l_Clients)
            {
                clientInfo += clientID + ", ";
            }
            clientInfo = clientInfo.TrimEnd(',', ' ');
            Console.WriteLine(clientInfo);
#endif

            // sleep thread for delay specified
            if (l_Delay > 0)
            {
                Console.WriteLine($"Process will begin at {DateTime.Now.AddMilliseconds(l_Delay).ToLongTimeString()}");
                Thread.Sleep(l_Delay);
            }

            do // will loop infintely if time interval is specified
            {
                Stopwatch l_StopWatch = Stopwatch.StartNew();
                Int64 l_TimeIntervalMinusRunTime = 0;
                DateTime l_RunTime = DateTime.UtcNow;
                DateTime l_LastRunTime = WorkflowVerification.GetLastRunTime();
                Int32 l_TotalClients = 0;
                Int32 l_VerificationsProcessing = 0;
                Dictionary<Int32, Queue<DocumentWorkflowItem>> l_ClientWorkflows = null;
                ConcurrentQueue<ItemModification> l_ItemsModified = new ConcurrentQueue<ItemModification>();
                Spinner l_Spinner;

                Console.Write("Querying ");
                Console.CursorVisible = false;
                l_Spinner = new Spinner(Console.CursorLeft, Console.CursorTop);
                l_Spinner.Start();

                // TODO: extract client workflows from DB after LatestVerification()
                try
                {
                    l_ClientWorkflows = new Dictionary<int, Queue<DocumentWorkflowItem>>(l_Clients.Count);
                    foreach (Int32 l_ClientID in l_Clients)
                    {
                        Queue<DocumentWorkflowItem> l_WorkflowItems = DocumentWorkflowItem.GetLastestForClient(l_ClientID, l_LastRunTime);
                    }
                    l_TotalClients = l_ClientWorkflows.Values.Count((v) => v.Count > 0);
                }
                catch (Exception e)
                {
                    Console.Write(e.Message);
                }

                l_Spinner.Stop("done");
                Console.Write("Making changes");
                if (!Directory.Exists(l_TempDirPath))
                    Directory.CreateDirectory(l_TempDirPath);

                try
                {
                    // new parent task for each verification child task
                    Task l_Process = new Task(() =>
                    {
                        foreach (KeyValuePair<Int32, Queue<DocumentWorkflowItem>> l_ClientWorkflow in l_ClientWorkflows)
                        {
                            if (l_ClientWorkflow.Value.Count == 0)
                                continue;

                            WorkflowVerification l_Verification = new WorkflowVerification(l_ClientWorkflow.Key, l_ClientWorkflow.Value);
                            l_VerificationsProcessing++;

                            // new task for each verification
                            new Task(() =>
                            {
                                Verify(l_Verification, l_TempDirPath, l_TotalClients);
                                if (l_Verification.Processed)
                                {
                                    Interlocked.Add(ref m_VerificationsProcessed, 1);
                                    l_Verification.ItemsModified.ForEach((i) => l_ItemsModified.Enqueue(i));
                                }
                            }, TaskCreationOptions.AttachedToParent).Start();
                        }
                    });

                    // start parent task, wait for children to finish executing
                    l_Process.Start();
                    l_Process.Wait();

                    // parent task finished, log results
                    l_StopWatch.Stop();

                    String l_ProcessResultsMessage =
                        $"Completed in {l_StopWatch.ElapsedMilliseconds}ms";

                    if (l_TimeInterval > 0)
                    {
                        l_TimeIntervalMinusRunTime = l_TimeInterval - l_StopWatch.ElapsedMilliseconds;
                        l_TimeIntervalMinusRunTime = (l_TimeIntervalMinusRunTime < 0) ?
                            0 :
                            l_TimeIntervalMinusRunTime;
                        l_ProcessResultsMessage +=
                            $" ... Next run at {DateTime.Now.AddMilliseconds(l_TimeIntervalMinusRunTime).ToLongTimeString()}";
                    }

                    // log any process results here all together
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                    Console.WriteLine($"{l_ItemsModified.Count} items modified for {m_VerificationsProcessed} clients");
                    foreach (ItemModification l_Mod in l_ItemsModified)
                    {
                        Console.WriteLine(l_Mod);
                    }
                    Console.WriteLine(l_ProcessResultsMessage);

                    // write log (id, runtime, clients (count), modifications (count), etc) to db
                    using (SqlConnection l_conn = DBHelp.CreateSQLConnection())
                    {
                        using (SqlCommand l_cmd = DBHelp.CreateCommand(l_conn, "WorkflowVerification_Add"))
                        {
                            l_cmd.Parameters.AddWithValue("@a_RunTime", l_RunTime);
                            l_cmd.Parameters.AddWithValue("@a_Summary", $"{l_ItemsModified.Count} items modified for {m_VerificationsProcessed} clients");
                            l_conn.Open();
                            l_cmd.ExecuteNonQuery();
                        }
                    }

                    // sleep thread for the fixed time interval
                    Console.CursorVisible = true;
                    Thread.Sleep(Convert.ToInt32(l_TimeIntervalMinusRunTime));
                }
                catch (Exception e)
                {
                    Console.Write(e.Message);
                }

            } while (l_TimeInterval > 0);
        }
        private static WorkflowVerification Verify(WorkflowVerification a_Verification, String a_TempDirPath, Int32 a_TotalClients)
        {
            Int32 a_ClientID = a_Verification.ClientID;
            Queue<DocumentWorkflowItem> a_Workflow = a_Verification.Workflow;
            Nullable<DateTime> a_RunTime = a_Verification.RunTime;
            String l_AwsS3LogPath = "test" + a_ClientID + "/" + DateTime.Now.ToFileTimeUtc();
            VerificationLogger l_Logger = new VerificationLogger(l_AwsS3LogPath, a_RunTime, a_TempDirPath);
            Stopwatch l_StopWatch = new Stopwatch();
            Int32 l_Percentage = 0;

            // append assignments for this Verification
            l_StopWatch.Start();
            a_Verification.AppendAssignments();
            l_StopWatch.Stop();
            a_Verification.RunSuccess = true;
            a_Verification.ElapsedTime = l_StopWatch.ElapsedMilliseconds;
            l_Percentage = ((m_VerificationsProcessed + 1) * 100) / a_TotalClients;

            // log to console
            if (m_VerificationsProcessed != 0)
                Console.SetCursorPosition(0, Console.CursorTop - 2);
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.WriteLine($"Making changes ({m_VerificationsProcessed + 1}/{a_TotalClients})");
            Console.WriteLine(ProgressTracker.ShowProgress(l_Percentage));

            return a_Verification;
        }
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.Write(e.ExceptionObject.ToString());

            if (m_LogToS3)
            {
                var l_TempDirPath = ConfigurationManager.AppSettings["TempFileDirectory"];
                VerificationLogger l_Logger = new VerificationLogger(DateTime.Today.ToString("yyMMdd"), DateTime.Now, l_TempDirPath);
                l_Logger.AddEntry("Unknown", false, "Unknown Error Occurred.", (Exception)e.ExceptionObject);
                l_Logger.Save();
            }

#if Test
            Console.ReadKey();
#endif
            Environment.Exit(1);
        }
        private static void Test()
        {
            DateTime? time = DateTime.Now;

            WorkflowVerification test = new WorkflowVerification(36);
            //test.Workflow = DocumentWorkflowItem.GetLastestForClient(test.ClientID, DateTime.Now.AddHours(-1));

            //foreach(DocumentWorkflowItem item in test.Workflow)
            //{
            //    Console.WriteLine(item.ToString());
            //}
            DocumentWorkflowItem item = new DocumentWorkflowItem(222);
            Console.WriteLine(item.ToString());
        }
    }
}