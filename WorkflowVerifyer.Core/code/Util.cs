using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;

namespace WorkflowVerifyer.Core
{
    /// <summary>
    /// A stateless class that provides utility methods to be used across a project
    /// </summary>
    public class Util
    {
        public static Amazon.RegionEndpoint AwsS3Region { get => Amazon.RegionEndpoint.GetBySystemName(ConfigurationManager.AppSettings["AwsS3Region"]); }

        /// <summary>
        /// Generates a random alphanumeric string a_Length characters long
        /// </summary>
        /// <param name="a_Length"></param>
        /// <returns>Returns the random alphanumeric string</returns>
        public static String GenerateRandomAlphaNumericString(Int32 a_Length)
        {
            String l_CharacterPool = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random l_Random = new Random();
            Char[] l_AlphaNumericCharArray = Enumerable
                                            .Repeat(l_CharacterPool, a_Length)
                                            .Select(s => s[l_Random.Next(s.Length)])
                                            .ToArray();

            return new String(l_AlphaNumericCharArray);
        }

        /// <summary>
        /// Uploads a file to AWS S3
        /// </summary>
        /// <param name="a_BucketName"></param>
        /// <param name="a_FileBytes"></param>
        /// <param name="a_FilePath"></param>
        /// <param name="a_ErrorMessage"></param>
        /// <returns>Returns true if uploaded properly, else false</returns>
        public static bool UploadAwsS3File(String a_BucketName, byte[] a_FileBytes, String a_FilePath, ref String a_ErrorMessage)
        {
            try
            {
                using (TransferUtility l_FileTransferUtility = new TransferUtility(GetAwsS3Client()))
                {
                    l_FileTransferUtility.Upload(new MemoryStream(a_FileBytes), a_BucketName, a_FilePath.Replace(@"\", "/"));
                }
            }
            catch (AmazonS3Exception ex)
            {
                a_ErrorMessage = ex.Message + " ; ErrorCode : " + ex.ErrorCode + " ; ErrorType : " + ex.ErrorType;
                return false;
            }
            catch (Exception ex)
            {
                a_ErrorMessage = ex.Message;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets an array of bytes for the specified AWS S3 file path
        /// </summary>
        /// <param name="a_BucketName"></param>
        /// <param name="a_FilePath"></param>
        public static Byte[] GetAwsS3FileBytes(String a_BucketName, String a_FilePath)
        {
            Byte[] l_FileBytes = null;
            try
            {
                using (TransferUtility l_FileTransferUtility = new TransferUtility(GetAwsS3Client()))
                {
                    using (MemoryStream l_Ms = new MemoryStream())
                    {
                        l_FileTransferUtility.OpenStream(a_BucketName, a_FilePath).CopyTo(l_Ms);
                        l_FileBytes = l_Ms.ToArray();
                    }
                }
            }
            catch (AmazonS3Exception ex)
            {
                if (ex.ErrorCode != "NoSuchKey")
                    throw new Exception(ex.Message, ex.InnerException);
            }
            return l_FileBytes;
        }

        /// <summary>
        /// Returns the AmazonS3Client Object specified in App.config AppSettings
        /// </summary>
        private static AmazonS3Client GetAwsS3Client()
        {
            String l_AccessKeyID = ConfigurationManager.AppSettings["AwsS3AccessKeyID"];
            String l_SecretAccessKey = ConfigurationManager.AppSettings["AwsS3SecretAccessKey"];
            String l_Region = ConfigurationManager.AppSettings["AwsS3Region"];

            AWSCredentials l_AWSCredentials = new BasicAWSCredentials(l_AccessKeyID, l_SecretAccessKey);
            AmazonS3Client l_AmazonS3Client = new AmazonS3Client(l_AWSCredentials, AwsS3Region);

            return l_AmazonS3Client;
        }

        /// <summary>
        /// Returns the AWS S3 public file URL
        /// </summary>
        /// <param name="a_BucketName"></param>
        /// <param name="a_FilePath"></param>
        public static String GetAwsS3FilePublicUrl(Object a_BucketName, Object a_FilePath)
        {
            // return "http://" + a_BucketName + ".s3-website-us-east-1.amazonaws.com/" + a_FilePath;
            return $"http://{a_BucketName}.s3-website-{ConfigurationManager.AppSettings["AwsS3Region"]}.amazonaws.com/{a_FilePath}";
        }
    }
}