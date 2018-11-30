#define Log
//#define Test

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WorkflowVerifyer.App.Helpers;

namespace WorkflowVerifyer.App
{
    internal class Program
    {
        private static Int32 m_ClientsProcessed;
        private static Int32 m_ClientsWhereChangesMade;
        private static Boolean m_LogToS3;

        private static int Main(String[] a_Args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            Console.WriteLine("Starting application...");

            Run(a_Args);
#if DEBUG
            Console.ReadKey();
#endif
            return 0;
        }
        private static void Run(String[] a_Args)
        {
            Dictionary<String, Object> l_ArgValuePairs = ArgumentExtraction.ExtractArgValuePairs(a_Args);
            Int64 l_TimeInterval = Convert.ToInt64(l_ArgValuePairs["TimeInterval"]);
            String l_ClientArgumentString = (l_ArgValuePairs["Client"]).ToString();
            List<Int32> l_Clients = ArgumentExtraction.ReturnClients(l_ClientArgumentString);
            Int32 l_Delay = Convert.ToInt32(l_ArgValuePairs["Delay"]);
            String l_TempDirPath = ConfigurationManager.AppSettings["TempFileDirectory"];
            m_LogToS3 = l_ArgValuePairs["LogToS3"].ToString() == "1";

#if Log
            String l_ClientInfo = "Clients specified: ";
            l_Clients.ForEach((l_ID) => l_ClientInfo += l_ID + ", ");
            l_ClientInfo = l_ClientInfo.TrimEnd(',', ' ');

            Console.WriteLine($"TimeInterval: {l_TimeInterval}");
            Console.WriteLine($"Delay: {l_Delay}");
            Console.WriteLine($"LogToS3: {m_LogToS3}");
            Console.WriteLine(l_ClientInfo);
            Console.WriteLine();

            Thread.Sleep(3000); // 3 second delay by default incase any args need to be changed
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
                Int64 l_TimeIntervalMinusRunTime = l_TimeInterval;
                DateTime l_RunTime = DateTime.Now;
                DateTime l_LastRunTime = WorkflowVerification.GetLastRunTime().AddYears(-1);
                Int32 l_TotalClients = 0, l_VerificationsProcessing = 0;
                Dictionary<Int32, List<DocumentWorkflowItem>> l_ClientWorkflows = null;
                ConcurrentQueue<ItemModification> l_ItemsModified = new ConcurrentQueue<ItemModification>();
                Spinner l_Spinner;
                m_ClientsProcessed = m_ClientsWhereChangesMade = 0;

                Console.WriteLine("Initiating: " + l_RunTime.ToLongTimeString());
                Console.Write("Reading from database ");
                Console.CursorVisible = false;
                l_Spinner = new Spinner(Console.CursorLeft, Console.CursorTop);
                l_Spinner.Start();

                // extract client workflows from DB after latest verification
                try
                {
                    l_ClientWorkflows = new Dictionary<Int32, List<DocumentWorkflowItem>>(l_Clients.Count);
                    foreach (Int32 l_ClientID in l_Clients)
                    {
                        List<DocumentWorkflowItem> l_WorkflowItems = DocumentWorkflowItem.GetLastestForClient(l_ClientID, l_LastRunTime);
                        l_ClientWorkflows.Add(l_ClientID, l_WorkflowItems);
                    }
                    l_TotalClients = l_ClientWorkflows.Values.Count((v) => v.Count > 0);
                }
                catch (Exception e)
                {
                    // we cannot continue the program if items are not able to be read from the db
                    ErrorHandler(e, true);
                }

                l_Spinner.Stop("done");
                Console.Write("Looking through workflows");
                if (!Directory.Exists(l_TempDirPath))
                    Directory.CreateDirectory(l_TempDirPath);

                try
                {
                    // new parent task for each verification child task
                    Task l_Process = new Task(() =>
                    {
                        foreach (KeyValuePair<Int32, List<DocumentWorkflowItem>> l_ClientWorkflow in l_ClientWorkflows)
                        {
                            if (l_ClientWorkflow.Value.Count == 0)
                                continue;

                            WorkflowVerification l_Verification = new WorkflowVerification(l_ClientWorkflow.Key, l_ClientWorkflow.Value);
                            l_VerificationsProcessing++;

                            // new task for each verification
                            new Task(() =>
                            {
                                if (Verify(l_Verification, l_TempDirPath, l_TotalClients).Processed)
                                {
                                    Interlocked.Add(ref m_ClientsProcessed, 1);
                                    l_Verification.ItemsModified.ForEach((i) => l_ItemsModified.Enqueue(i));

                                    if (l_Verification.ChangesMade)
                                    {
                                        Interlocked.Add(ref m_ClientsWhereChangesMade, 1);
                                    }
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
                        l_TimeIntervalMinusRunTime = (l_TimeIntervalMinusRunTime < 0) ? 0 : l_TimeIntervalMinusRunTime;
                        l_ProcessResultsMessage +=
                            $" ... Next run at {DateTime.Now.AddMilliseconds(l_TimeIntervalMinusRunTime).ToLongTimeString()}\n";
                    }

                    // log any process results here all together
                    StringBuilder l_LogString = new StringBuilder();
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                    Console.WriteLine($"{l_ItemsModified.Count} item modification(s) for {m_ClientsWhereChangesMade} client(s)");

                    while (l_ItemsModified.Any())
                    {
                        l_ItemsModified.TryDequeue(out ItemModification l_Mod);

                        // TODO: write to db
                        if (!l_Mod.WriteToDB())
                        {
                            ErrorHandler(new Exception("Could not write modifications to the databasse"), true);
                        }

                        // write to console
                        Console.WriteLine(l_Mod);

                        // add to log string
                        l_LogString.Append(l_Mod + "\n");
                    }

                    Console.WriteLine(l_ProcessResultsMessage);

                    // write log (id, runtime, clients (count), modifications (count), etc) to db
                    using (SqlConnection l_conn = DBHelp.CreateSQLConnection())
                    {
                        using (SqlCommand l_cmd = DBHelp.CreateCommand(l_conn, "WorkflowVerification_Add"))
                        {
                            l_cmd.Parameters.AddWithValue("@a_RunTime", l_RunTime);
                            l_cmd.Parameters.AddWithValue("@a_Summary", $"{l_ItemsModified.Count} item modification(s) {m_ClientsWhereChangesMade} client(s)");
                            l_conn.Open();
                            l_cmd.ExecuteNonQuery();
                        }
                    }

                    // if logging to S3, use log string
                    if (m_LogToS3)
                    {
                        String l_AwsS3LogPath = "log" + "/" + DateTime.Now.ToFileTimeUtc();

                        using (VerificationLogger l_Logger = new VerificationLogger(l_AwsS3LogPath, DateTime.Now, l_TempDirPath))
                        {
                            //l_Logger.AddEntry("Unknown", false, "Unknown Error Occurred.", (Exception)e.ExceptionObject);
                            l_Logger.AddEntry("Workflow Verification Processed", true, l_LogString.ToString());
                            l_Logger.Save();
                        }
                    }
                }
                catch (Exception e)
                {
                    ErrorHandler(e);
                }

                // sleep thread for the fixed time interval
                Console.CursorVisible = true;
                Thread.Sleep(Convert.ToInt32(l_TimeIntervalMinusRunTime));

            } while (l_TimeInterval > 0);
        }
        private static WorkflowVerification Verify(WorkflowVerification a_Verification, String a_TempDirPath, Int32 a_TotalClients)
        {
            a_Verification.Processed = true;

            Int32 a_ClientID = a_Verification.ClientID;
            List<DocumentWorkflowItem> a_Workflow = a_Verification.Workflow;
            Nullable<DateTime> a_RunTime = a_Verification.RunTime;
            Stopwatch l_StopWatch = new Stopwatch();
            Int32 l_Percentage = 0;

            Thread.Sleep(a_Verification.ClientID * 10); // this line prevents completions at the same times

            // append assignments for this Verification
            l_StopWatch.Start();
            a_Verification.AppendAssignments();
            l_StopWatch.Stop();
            a_Verification.RunSuccess = true;
            a_Verification.ElapsedTime = l_StopWatch.ElapsedMilliseconds;
            l_Percentage = ((m_ClientsProcessed + 1) * 100) / a_TotalClients;

            // log to console
            if (m_ClientsProcessed != 0)
            {
                Console.SetCursorPosition(0, Console.CursorTop - 2);
            }
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.WriteLine($"Looking through workflows ({m_ClientsProcessed + 1}/{a_TotalClients})");
            Console.WriteLine(ProgressTracker.ShowProgress(l_Percentage));

            return a_Verification;
        }
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ErrorHandler((Exception)e.ExceptionObject);
#if DEBUG
            Console.ReadKey();
#endif
            Environment.Exit(1);
        }
        private static void ErrorHandler(Exception e, Boolean environmentExit = false)
        { 
            Console.WriteLine("////////");
            Console.WriteLine(e.Message);
            Console.WriteLine("////////");

            if (m_LogToS3)
            {
                String l_AwsS3LogPath = "error" + "/" + DateTime.Now.ToFileTimeUtc();
                String l_TempDirPath = ConfigurationManager.AppSettings["TempFileDirectory"];

                using (VerificationLogger l_Logger = new VerificationLogger(l_AwsS3LogPath, DateTime.Now, l_TempDirPath))
                {
                    l_Logger.AddEntry("Unknown", false, "Unknown Error Occurred.", e);
                    l_Logger.Save();
                }
            }

            if (environmentExit) Environment.Exit(1);
        }
    }
}