using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using WorkflowVerifyer.App.Helpers;
using WorkflowVerifyer.Core;

namespace WorkflowVerifyer.App
{
    internal class Program
    {
        private static volatile Int32 SuccessfulVerifications;
        private static volatile ConsoleSpinner Spinner;

        private static void Main(String[] a_Args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            Int64 l_TimeInterval;
            Int32 l_Delay;
            Dictionary<String, Object> l_ArgValuePairs = ArgumentExtraction.ExtractArgValuePairs(a_Args);
            Object[] l_ProcessData = { DateTime.Now, ArgumentExtraction.ExtractArgValuePairs(a_Args) }; // TODO: log this to S3

            if (!ArgumentExtraction.ValidateArgs(l_ProcessData[1] as Dictionary<String, Object>)) return;

            // args valid, begin operation
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
                String l_TempDirPath = ConfigurationManager.AppSettings["TempFileDirectory"];
                SuccessfulVerifications = 0;
                Spinner = new ConsoleSpinner(0, 0);

                // TODO: get all verification records from db, store as DataTable (records will be dt, foreach will iterate rows in dt.rows
                String[] l_Records = { "1", "2", "3", "4", "5" }; 
                Thread.Sleep(1000); // to simiulate loading time for querying from db

                // new parent task for each verification child task
                Task l_Process = new Task(() =>
                {
                    // TODO: replace for with foreach to iterate over DataRow vars in DataTable Rows, allocate first index of l_data to current DataRow var
                    for (int i = 0; i < l_Records.Length; i++)
                    {
                        Object[] l_ObjectData = { l_Records[i], DateTime.Now, l_TempDirPath };

                        // new task for each verification
                        new Task(() => { if(Verify(l_ObjectData)) ++SuccessfulVerifications; }, TaskCreationOptions.AttachedToParent).Start();
                    }
                });

                // start parent task, wait for children to finish executing
                Spinner.Start();
                l_Process.Start();
                l_Process.Wait();

                // parent task finished, log results
                l_StopWatch.Stop();

                String l_ProcessResultsMessage = $"Process completed in {l_StopWatch.ElapsedMilliseconds}ms; {SuccessfulVerifications}/{l_Records.Length} verifications successful";
                if (l_TimeInterval > 0)
                {
                    l_TimeIntervalMinusRunTime = l_TimeInterval - l_StopWatch.ElapsedMilliseconds;
                    l_TimeIntervalMinusRunTime = (l_TimeIntervalMinusRunTime < 0) ? 0 : l_TimeIntervalMinusRunTime;
                    l_ProcessResultsMessage += $" ... Next run in {l_TimeIntervalMinusRunTime}ms";
                }
                Spinner.Stop(l_ProcessResultsMessage);

                // sleep thread for the fixed time interval
                Thread.Sleep(Convert.ToInt32(l_TimeIntervalMinusRunTime));

            } while (l_TimeInterval > 0);
        }
        
        private static Boolean Verify(Object[] a_ObjectData)
        {
            String a_ClientID = a_ObjectData[0] as String;
            Nullable<DateTime> a_RunTime = a_ObjectData[1] as Nullable<DateTime>;
            String a_TempDirPath = a_ObjectData[2] as String;
            
            Verification l_VerificationResult = new Verification(a_ClientID);
            Stopwatch l_StopWatch = new Stopwatch();
            Boolean l_RunSuccess = true;
            String l_AwsS3LogPath = "test" + a_ClientID + "/" + DateTime.Now.ToFileTimeUtc();
            VerificationLogger l_Logger = new VerificationLogger(l_AwsS3LogPath, a_RunTime, a_TempDirPath);

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

                l_Logger.AddEntry("Unknown", false, "Unknown error occurred during workflow verification for client '" + l_VerificationResult.ClientID + "'.", e);
                l_Logger.Save();

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
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.Write(e.ExceptionObject.ToString());
            var l_TempDirPath = ConfigurationManager.AppSettings["TempFileDirectory"];
            VerificationLogger l_Logger = new VerificationLogger(DateTime.Today.ToString("yyMMdd"), DateTime.Now, l_TempDirPath);
            l_Logger.AddEntry("Unknown", false, "Unknown Error Occurred.", (Exception)e.ExceptionObject);
            l_Logger.Save();
            Environment.Exit(1);

            //VerificationLogger.LogToS3(errorMessage);
        }
    }
}
