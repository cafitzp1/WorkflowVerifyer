#define Debug
#undef Debug
#define Log
#undef Log
#define Test
#undef Test
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using WorkflowVerifyer.App.Helpers;

namespace WorkflowVerifyer.App
{
    internal class Program
    {
        public static ManualResetEvent _Shutdown = new ManualResetEvent(false);
        public static ManualResetEventSlim _Complete = new ManualResetEventSlim();
        private static Boolean m_LogToS3;
        private static volatile List<WorkflowVerification> m_Verifications;

        private static int Main(String[] a_Args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            Console.WriteLine("Starting application...");
#if Test
            Test();
#else
            Run(a_Args);
#endif

            return 0;
        }
        private static void Run(String[] a_Args)
        {
            Dictionary<String, Object> l_ArgValuePairs = ArgumentExtraction.ExtractArgValuePairs(a_Args);

            // pull values from arguments
            Int64 l_TimeInterval = Convert.ToInt64(l_ArgValuePairs[nameof(ArgumentKey.TimeInterval)]);
            String l_ClientArgumentString = (l_ArgValuePairs[nameof(ArgumentKey.Client)]).ToString();
            List<Int32> l_Clients = ArgumentExtraction.ReturnClients(l_ClientArgumentString);
            Int32 l_Delay = Convert.ToInt32(l_ArgValuePairs[nameof(ArgumentKey.Delay)]);
            DateTime l_LastRunTime = DateTime.Now;
            m_LogToS3 = (l_ArgValuePairs[nameof(ArgumentKey.LogToS3)].ToString() == "1") ? true : false;

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
                String l_TempDirPath = ConfigurationManager.AppSettings["TempFileDirectory"];
                DateTime l_RunTime = DateTime.UtcNow;
                Spinner l_Spinner;

                Console.Write("Querying ");
                Console.CursorVisible = false;
                l_Spinner = new Spinner(Console.CursorLeft, Console.CursorTop);
                l_Spinner.Start();

                // TODO: extract client workflows from DB after LatestVerification()
                Thread.Sleep(3000);

                l_Spinner.Stop("done");
                Console.Write("Making changes");
                m_Verifications = new List<WorkflowVerification>();
                if (!Directory.Exists(l_TempDirPath))
                    Directory.CreateDirectory(l_TempDirPath);

                try
                {
                    // new parent task for each verification child task
                    Task l_Process = new Task(() =>
                    {
                        // FIXME: use records in dictionary for iteration (see comments right below)
                        foreach (Int32 l_ClientID in l_Clients)
                        {
                            // TODO: since we have workflow items from earlier, pass this client's collection
                            // maybe use a dictionary <Int32, List<WorkflowItem> (client id for int)
                            Object[] l_ObjectData = { l_ClientID, l_RunTime, l_TempDirPath, l_Clients };

                            // new task for each verification
                            new Task(() =>
                            {
                                m_Verifications.Add(Verify(l_ObjectData));
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

                    // log any process results here all together TODO: include start time and clients here
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                    Console.WriteLine($"Items modified for {m_Verifications.Count} clients:");
                    Int32 l_VerificationCount = 0;
                    foreach (WorkflowVerification l_Verification in m_Verifications)
                    {
                        foreach (ItemModification l_Mod in l_Verification.ItemsModified)
                        {
                            Console.WriteLine(l_Mod);
                            l_VerificationCount++;
                        }
                    }
                    Console.WriteLine(l_ProcessResultsMessage);

                    // write log (id, runtime, clients (count), modifications (count), etc) to db
                    using (SqlConnection l_conn = DBHelp.CreateSQLConnection())
                    {
                        using (SqlCommand l_cmd = DBHelp.CreateCommand(l_conn, "WorkflowVerification_Add"))
                        {
                            l_cmd.Parameters.AddWithValue("@a_RunTime", l_RunTime);
                            l_cmd.Parameters.AddWithValue("@a_Summary", $"{l_VerificationCount} items modified for {m_Verifications.Count} clients");
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
        private static WorkflowVerification Verify(Object[] a_ObjectData)
        {
            String a_ClientID = a_ObjectData[0].ToString();
            Nullable<DateTime> a_RunTime = a_ObjectData[1] as Nullable<DateTime>;
            String a_TempDirPath = a_ObjectData[2] as String;
            List<Int32> a_Clients = a_ObjectData[3] as List<Int32>;

            WorkflowVerification l_VerificationResult = new WorkflowVerification(a_ClientID);
            Stopwatch l_StopWatch = new Stopwatch();
            String l_AwsS3LogPath = "test" + a_ClientID + "/" + DateTime.Now.ToFileTimeUtc();
            VerificationLogger l_Logger = new VerificationLogger(l_AwsS3LogPath, a_RunTime, a_TempDirPath);
            Int32 l_Percentage;

            // apply any item modifications here
            l_StopWatch.Start();

            // NOTE: we have to implement logic for this
            // append assignments to this Verification object
            l_VerificationResult.AppendAssignments();
            // Thread.Sleep(Convert.ToInt32(a_ClientID) * 1000);

            // end, store results
            l_StopWatch.Stop();
            l_VerificationResult.RunSuccess = true;
            l_VerificationResult.ElapsedTime = l_StopWatch.ElapsedMilliseconds;
            l_Percentage = ((m_Verifications.Count + 1) * 100) / a_Clients.Count;

            // log to console
            if (m_Verifications.Count != 0) 
                Console.SetCursorPosition(0, Console.CursorTop - 2);
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.WriteLine($"Making changes ({m_Verifications.Count + 1}/{a_Clients.Count})");
            Console.WriteLine(ProgressTracker.ShowProgress(l_Percentage));

            return l_VerificationResult;
        }
        private static DateTime LatestVerification()
        {
            DateTime l_Latest = DateTime.Now;

            using (SqlConnection l_conn = DBHelp.CreateSQLConnection())
            {
                using (SqlCommand l_cmd = DBHelp.CreateCommand(l_conn, "WorkflowVerification_GetLatest"))
                {
                    l_conn.Open();
                    using (SqlDataReader l_rdr = l_cmd.ExecuteReader())
                    {
                        if (l_rdr.Read())
                        {
                            l_Latest = Convert.ToDateTime(l_rdr["RunTime"]);
                        }
                    }
                }
            }

            return l_Latest;
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

            Environment.Exit(1);
        }
        private static void Test()
        {
            DateTime? time = DateTime.Now;

            using (SqlConnection l_conn = DBHelp.CreateSQLConnection())
            {
                using (SqlCommand l_cmd = DBHelp.CreateCommand(l_conn, "WorkflowVerification_Add"))
                {
                    l_cmd.Parameters.AddWithValue("@a_RunTime", DateTime.Now);
                    l_cmd.Parameters.AddWithValue("@a_Summary", "Hello, this is a test");
                    l_conn.Open();
                    l_cmd.ExecuteNonQuery();
                }
            }

            using (SqlConnection l_conn = DBHelp.CreateSQLConnection())
            {
                using (SqlCommand l_cmd = DBHelp.CreateCommand(l_conn, "WorkflowVerification_GetLatest"))
                {
                    l_conn.Open();
                    using (SqlDataReader l_rdr = l_cmd.ExecuteReader())
                    {
                        if (l_rdr.Read())
                        {
                            time = Convert.ToDateTime(l_rdr["RunTime"]);
                        }
                    }
                }
            }

            Console.WriteLine("Time = " + time.Value.ToLongTimeString());
        }
    }
}