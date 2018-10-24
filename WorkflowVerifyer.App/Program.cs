using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using WorkflowVerifyer.App.Helpers;

namespace WorkflowVerifyer.App
{
    internal class Program
    {
        private static volatile Int32 m_SuccessfulVerifications;
        private static volatile ConsoleSpinner m_Spinner;

        private static void Main(String[] a_Args)
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            Int64 l_TimeInterval;
            Int32 l_Delay;
            Dictionary<String, Object> l_ArgValuePairs = ArgumentExtraction.ExtractArgValuePairs(a_Args);
            Object[] l_ProcessData = { DateTime.Now, ArgumentExtraction.ExtractArgValuePairs(a_Args) }; // TODO: log this to S3

            if (!ArgumentExtraction.ValidateArgs(l_ProcessData[1] as Dictionary<String, Object>)) return;

            // args valid, begin operation
            foreach (KeyValuePair<String, Object> entry in l_ProcessData[1] as Dictionary<String, Object>)
            {
                if (entry.Key == "unrecognized")
                {
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine($"{entry.Key}={entry.Value}");
                }
            }
            l_Delay = Convert.ToInt32((l_ProcessData[1] as Dictionary<String, Object>)[ArgumentKey.Delay]);
            l_TimeInterval = Convert.ToInt64((l_ProcessData[1] as Dictionary<String, Object>)[ArgumentKey.TimeInterval]);

            // sleep thread for delay specified
            if (l_Delay > 0)
            {
                Console.WriteLine($"Process will begin at {DateTime.Now.AddMilliseconds(l_Delay).ToLongTimeString()}");
                Thread.Sleep(l_Delay);
            }

            do // will loop infintely if time interval is specified
            {
                Int64 l_TimeIntervalMinusRunTime = 0;
                Stopwatch l_StopWatch = Stopwatch.StartNew();
                m_SuccessfulVerifications = 0;
                m_Spinner = new ConsoleSpinner(0, 0);

                // TODO: get all verification records from db, store as DataTable (records will be dt, foreach will iterate rows in dt.rows
                String[] l_Records = { "1", "2", "3", "4", "5" }; 
                Thread.Sleep(1000); // to simiulate loading time for querying from db

                // new parent task for each verification child task
                Task l_Process = new Task(() =>
                {
                    // TODO: replace for with foreach to iterate over DataRow vars in DataTable Rows, allocate first index of l_data to current DataRow var
                    for (int i = 0; i < l_Records.Length; i++)
                    {
                        String l_ClientID = l_Records[i];

                        // new task for each verification
                        new Task(() => { if(Verify(l_ClientID)) ++m_SuccessfulVerifications; }, TaskCreationOptions.AttachedToParent).Start();
                    }
                });

                // start parent task, wait for children to finish executing
                m_Spinner.Start();
                l_Process.Start();
                l_Process.Wait();

                // parent task finished, log results
                l_StopWatch.Stop();

                String l_ProcessResultsMessage = $"Process completed in {l_StopWatch.ElapsedMilliseconds}ms; {m_SuccessfulVerifications}/{l_Records.Length} verifications successful";
                if (l_TimeInterval > 0)
                {
                    l_TimeIntervalMinusRunTime = l_TimeInterval - l_StopWatch.ElapsedMilliseconds;
                    l_TimeIntervalMinusRunTime = (l_TimeIntervalMinusRunTime < 0) ? 0 : l_TimeIntervalMinusRunTime;
                    l_ProcessResultsMessage += $" ... Next run in {l_TimeIntervalMinusRunTime}ms";
                }
                m_Spinner.Stop(l_ProcessResultsMessage);

                // sleep thread for the fixed time interval
                Thread.Sleep(Convert.ToInt32(l_TimeIntervalMinusRunTime));

            } while (l_TimeInterval > 0);
        }
        
        private static Boolean Verify(String a_ClientID)
        {
            Verification l_VerificationResult = new Verification(a_ClientID);
            Stopwatch l_StopWatch = new Stopwatch();
            Boolean l_RunSuccess = true;

            try
            {
                l_StopWatch.Start();

                if (a_ClientID == "3") Convert.ToBoolean(a_ClientID);

                // FIXME: sleeping to simulate processing time; need to implement actual method logic here
                // TODO: should route to this specific client's verifyer class
                Thread.Sleep(Convert.ToInt32(a_ClientID) * 1000);

                l_StopWatch.Stop();
                l_VerificationResult.RunSuccess = true;
            }
            catch (Exception e)
            {
                l_StopWatch.Stop();
                l_VerificationResult.RunSuccess = false;
                l_RunSuccess = false;

                LogError(e);
            }
            finally
            {
                l_VerificationResult.ElapsedTime=l_StopWatch.ElapsedMilliseconds;
                // m_Spinner.Stop($"{l_VerificationResult}");
                // m_Spinner = new ConsoleSpinner(0, 0);
                // m_Spinner.Start();
            }

            return l_RunSuccess;
        }
        public static void LogError(Exception e)
        {
            DateTime timeOfError = DateTime.Now;
            String errorMessage = $@"
                ERROR: Occurred at {timeOfError}
                Exception: {e.Message}
                Trace: {e.StackTrace}
            ";

            //VerificationLogger.LogToS3(errorMessage);
        }
        public static void LogError(String message)
        {
            DateTime timeOfError = DateTime.Now;
            String errorMessage = $@"
                ERROR: Occurred at {timeOfError}
                Message: {message}
            ";

            //VerificationLogger.LogToS3(errorMessage);
        }
        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            // handler is entered on program completion
        }
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            m_Spinner.Stop();

            Exception ex = e.ExceptionObject as Exception;
            DateTime timeOfError = DateTime.Now;
            String errorMessage = $@"
                ERROR: Occurred at {timeOfError}
                Exception: {ex.Message}
                Trace: {ex.StackTrace}
            ";

            //VerificationLogger.LogToS3(errorMessage);
        }
    }
}
