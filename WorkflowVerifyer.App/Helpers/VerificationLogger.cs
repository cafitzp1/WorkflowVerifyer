using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon.Runtime.CredentialManagement;
using WorkflowVerifyer.Core;

namespace WorkflowVerifyer.App.Helpers
{
    internal class VerificationLogger
    {
        private static Amazon.RegionEndpoint AWS_S3_REGION = Amazon.RegionEndpoint.USWest2;

        private string m_LogString;
        private string m_AwsFileDir;
        private string m_LocalFileDir;
        private DateTime? m_LogDateTime;

        public VerificationLogger(String a_AwsS3Path, DateTime? a_LogDateTime, String a_LocalFileDir)
        {
            m_LocalFileDir = a_LocalFileDir;
            m_LogString = String.Empty;
            m_AwsFileDir = a_AwsS3Path;
            m_LogDateTime = a_LogDateTime;
        }

        public void AddEntry(string a_Action, bool a_WasSuccessful, string a_Message, Exception a_Exception = null)
        {
            DateTime l_DateTime;
            String l_Result;

            l_DateTime = (m_LogDateTime.HasValue) ? m_LogDateTime.Value : DateTime.Now;
            l_Result = (a_WasSuccessful) ? "Success" : "Error";

            if (a_Exception != null)
            {
                // TODO: upload file to 'error' path, not 'logs' - trigger notification on upload to this folder
                SystemErrorLog l_ErrorLog = SystemErrorLog.ReturnErrorLog(a_Exception);
                a_Message += l_ErrorLog.ErrorDescription;
            }

            m_LogString += l_DateTime + " , " + a_Action + " , " + l_Result;

            if (a_Message.Trim().Length > 0)
                m_LogString += " , " + a_Message;

            m_LogString += "\r\n";
        }

        public void Save()
        {
            if (m_LogString.Trim().Length > 0)
            {
                // TODO: logs should be in folders for each client; should be a master log for each run (ie 4/5 successful, time, etc.)

                // new log for each verification; 
                string l_FileName = $"{m_LogDateTime.Value.ToFileTimeUtc()}_log.txt";
                string l_TempFilePath = m_LocalFileDir + l_FileName;
                string l_S3Bucket = ConfigurationManager.AppSettings["LogsAwsS3BucketName"];

                File.AppendAllText(l_TempFilePath, m_LogString);
               
                UploadFileToS3(l_FileName);
            }
        }

        private SystemErrorLog LogSystemError(Exception a_Exception)
        {
            return SystemErrorLog.ReturnErrorLog(a_Exception);
        }

        
    }
}

