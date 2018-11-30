using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon.Runtime.CredentialManagement;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace WorkflowVerifyer.App.Helpers
{
    internal class VerificationLogger : IDisposable
    {
        public static Amazon.RegionEndpoint AwsS3Region { get => Amazon.RegionEndpoint.GetBySystemName(ConfigurationManager.AppSettings["AwsS3Region"]); }
        public string LogString { get; private set; }
        public string AwsFileDir { get; private set; }
        public string LocalFileDir { get; private set; }
        public DateTime? LogDateTime { get; private set; }
        public Boolean Disposed { get; private set; }
        SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);

        /// <summary>
        /// Constructs a VerificationLogger object
        /// </summary>
        /// <param name="a_AwsS3Path"></param>
        /// <param name="a_LogDateTime"></param>
        /// <param name="a_LocalFileDir"></param>
        public VerificationLogger(String a_AwsS3Path, DateTime? a_LogDateTime, String a_LocalFileDir)
        {
            LocalFileDir = a_LocalFileDir;
            LogString = String.Empty;
            AwsFileDir = a_AwsS3Path;
            LogDateTime = a_LogDateTime;
        }

        /// <summary>
        /// Appends to the log string member
        /// </summary>
        /// <param name="a_Action"></param>
        /// <param name="a_WasSuccessful"></param>
        /// <param name="a_Message"></param>
        /// <param name="a_Exception"></param>
        public void AddEntry(string a_Action, bool a_WasSuccessful, string a_Message, Exception a_Exception = null)
        {
            DateTime l_DateTime;
            String l_Result;

            l_DateTime = (LogDateTime.HasValue) ? LogDateTime.Value : DateTime.Now;
            l_Result = (a_WasSuccessful) ? "Success" : "Error";

            if (a_Exception != null)
            {
                // TODO: upload file to 'error' path, not 'logs' - trigger notification on upload to this folder

                SystemErrorLog l_ErrorLog = SystemErrorLog.ReturnErrorLog(a_Exception);
                a_Message += " " + l_ErrorLog.ErrorDescription;
            }

            LogString += l_DateTime + ", " + a_Action + ", " + l_Result;

            if (a_Message.Trim().Length > 0)
                LogString += "\n" + a_Message;

            LogString += "\r\n";
        }

        /// <summary>
        /// Creates a temporary file containing the log string and attempts an upload to AWS S3
        /// </summary>
        public void Save()
        {
            if (LogString.Trim().Length > 0)
            {
                // TODO: logs should be in folders for each client; should be a master log for each run (ie 4/5 successful, time, etc.)

                // new log for each verification; 
                string l_FileName = "log.txt";
                string l_TempFilePath = LocalFileDir + l_FileName;
                string l_S3Bucket = ConfigurationManager.AppSettings["LogsAwsS3BucketName"];

                File.AppendAllText(l_TempFilePath, LogString);

                UploadFileToS3(l_FileName);
            }
        }

        /// <summary>
        /// Gets a SystemErrorLog object with the exception error message instantiated
        /// </summary>
        /// <param name="a_Exception"></param>
        private SystemErrorLog LogSystemError(Exception a_Exception) => SystemErrorLog.ReturnErrorLog(a_Exception);

        /// <summary>
        /// Attempts to upload a file to AWS S3
        /// </summary>
        /// <param name="a_FileName"></param>
        public void UploadFileToS3(String a_FileName)
        {
            string l_LocalPath = LocalFileDir + a_FileName;
            string l_AwsFilePath = AwsFileDir + a_FileName;
            string l_S3Bucket = ConfigurationManager.AppSettings["LogsAwsS3BucketName"];
            string l_ErrorMessage = "";

            if (Util.UploadAwsS3File(l_S3Bucket, File.ReadAllBytes(l_LocalPath), l_AwsFilePath, ref l_ErrorMessage))
            {
                File.Delete(l_LocalPath);
            }
            else
            {
                // file failed to upload; we are renaming the file using the Aws directory structure m_AwsFileDir (integrator_name/date/upload/file_name) so it does not get overwritten
                //integrator_name/date/upload/file_name get's changed to integrator_name==date==upload==file_name)
                a_FileName = AwsFileDir.Replace("/", "==") + a_FileName;
                File.Move(l_LocalPath, LocalFileDir + a_FileName);
                SystemErrorLog l_SystemErrorLog = new SystemErrorLog();
                l_SystemErrorLog.ErrorDescription = "Error uploading S3 Cloud file - " + a_FileName + ":" + l_ErrorMessage;
                l_SystemErrorLog.ErrorID = Util.GenerateRandomAlphaNumericString(8);

                AddEntry("Upload", false, "Error uploading S3 Cloud file - " + a_FileName + ":" + l_ErrorMessage);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (Disposed)
                return;

            if (disposing)
            {
                handle.Dispose();
            }

            Disposed = true;
        }
    }
}

