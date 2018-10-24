using System;
using System.Collections.Generic;
using System.Configuration;
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
        private string m_ErrorEmailBody;
        private string m_AwsFileDir;
        private string m_LocalFileDir;
        private DateTime? m_LogDateTime;
        private List<String> m_ErrorEmailToAddresses;

        public VerificationLogger(string a_AwsS3Path, DateTime? a_LogDateTime, string a_LocalFileDir)
        {
            m_LocalFileDir = a_LocalFileDir;
            m_ErrorEmailBody = String.Empty;
            m_LogString = String.Empty;
            m_AwsFileDir = a_AwsS3Path;
            m_LogDateTime = a_LogDateTime;
            m_ErrorEmailToAddresses = new List<String>();

            Util.Test();
        }
    }
}

