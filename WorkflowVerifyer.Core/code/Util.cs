using System;
using System.Configuration;
using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon.Runtime.CredentialManagement;

namespace WorkflowVerifyer.App.Helpers
{
    internal class Util
    {
        
        public static void LogToS3(String message)
        {
            try
            {
                using (TransferUtility l_FileTransferUtility = new TransferUtility(GetAwsS3Client()))
                {
                    // ...
                }
            }
            catch (AmazonS3Exception) {}
            catch (Exception) {}
        }

        private static AmazonS3Client GetAwsS3Client()
        {
            // will read "AWSProfileName" from App.config and find stored access information from there
            AmazonS3Client l_AmazonS3Client = new AmazonS3Client();

            return l_AmazonS3Client;
        }
    }
}