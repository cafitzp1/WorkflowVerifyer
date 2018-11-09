#define Debug
#undef Debug
#define Log
#undef Log
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WorkflowVerifyer.App.Helpers;

namespace WorkflowVerifyer.App
{
    internal class Program
    {
        private static volatile List<WorkflowVerification> m_Verifications;
        private static Boolean m_LogToS3;

        private static void Main(String[] a_Args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

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
                l_Spinner = new Spinner(Console.CursorLeft, Console.CursorTop);
                l_Spinner.Start();

                // extract client workflow from DB
                Thread.Sleep(3000);

                l_Spinner.Stop("done");
                m_Verifications = new List<WorkflowVerification>();
                if (!Directory.Exists(l_TempDirPath))
                    Directory.CreateDirectory(l_TempDirPath);

                try
                {
                    // new parent task for each verification child task
                    Task l_Process = new Task(() =>
                    {
                        foreach (Int32 l_ClientID in l_Clients)
                        {
                            Object[] l_ObjectData = { l_ClientID, l_RunTime, l_TempDirPath, l_Clients };

                            // new task for each verification
                            new Task(() =>
                            {
                                m_Verifications.Add(Verify(l_ObjectData));
                            }, TaskCreationOptions.AttachedToParent).Start();
                        }
                    });

                    // start parent task, wait for children to finish executing
                    // TODO: before beginning the tasks, query workflow items altogether from db, save as member
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
                    foreach (WorkflowVerification l_Verification in m_Verifications)
                    {
                        foreach (ItemModification l_Mod in l_Verification.ItemsModified)
                        {
                            Console.WriteLine(l_Mod);
                        }
                    }
                    Console.WriteLine(l_ProcessResultsMessage);

                    // store runtime the most recent run time in appSettings
                    l_LastRunTime = l_RunTime;

                    // sleep thread for the fixed time interval
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
            Thread.Sleep(Convert.ToInt32(a_ClientID) * 1000);

            // end, store results
            l_StopWatch.Stop();
            l_VerificationResult.RunSuccess = true;
            l_VerificationResult.ElapsedTime = l_StopWatch.ElapsedMilliseconds;
            l_Percentage = ((m_Verifications.Count + 1) * 100) / a_Clients.Count;

            // log to console
            if (m_Verifications.Count != 0) Console.SetCursorPosition(0, Console.CursorTop - 2);
            Console.WriteLine($"Making changes ({m_Verifications.Count + 1}/{a_Clients.Count})");
            Console.WriteLine(ProgressTracker.ShowProgress(l_Percentage));

            return l_VerificationResult;
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
    }
}