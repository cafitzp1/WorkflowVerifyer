using System;
using System.Configuration;
using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon.Runtime.CredentialManagement;

namespace WorkflowVerifyer.Core
{
    public class Util
    {
        public static bool UploadAwsS3File(string a_BucketName, byte[] a_FileBytes, string a_FilePath, ref string a_ErrorMessage)
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

        public static byte[] GetAwsS3FileBytes(string a_BucketName, string a_FilePath)
        {
            byte[] l_FileBytes = null;
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

        public static bool AwsS3FileExists(string a_BucketName, string a_FileURL)
        {
            var l_DirectoryInfo = new Amazon.S3.IO.S3DirectoryInfo(GetAwsS3Client, a_BucketName);
            var l_File = l_DirectoryInfo.GetFile(a_FileURL);
            if ((l_File.Exists))
                return true;
            else
                return false;
        }

        public static bool AwsS3BucketExists(string a_BucketName)
        {
            try
            {
                AmazonS3Client l_AmazonS3Client = GetAwsS3Client();
                S3DirectoryInfo l_DirInfo = new S3DirectoryInfo(l_AmazonS3Client, a_BucketName);

                S3DirectoryInfo[] l_Rest = l_DirInfo.GetDirectories();
                return l_Rest.Count > 0;
            }
            catch (AmazonS3Exception ex)
            {
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool DeleteAwsS3File(string a_BucketName, string a_FilePath, ref string a_ErrorMessage)
        {
            try
            {
                AmazonS3Client l_AmazonS3Client = GetAwsS3Client();
                l_AmazonS3Client.DeleteObject(a_BucketName, a_FilePath);
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

        public static bool DeleteAwsS3Files(string a_BucketName, ArrayList a_FilePaths, ref string a_ErrorMessage)
        {
            try
            {
                var l_Request = new Model.DeleteObjectsRequest();
                l_Request.BucketName = a_BucketName;
                foreach (string l_Files in a_FilePaths)
                    l_Request.AddKey(l_Files);

                AmazonS3Client l_AmazonS3Client = GetAwsS3Client();
                Model.DeleteObjectsResponse l_Response = l_AmazonS3Client.DeleteObjects(l_Request);

                if (l_Response.DeleteErrors.Count > 0)
                {
                    foreach (var l_Error in l_Response.DeleteErrors)
                        a_ErrorMessage += " File path " + l_Error.Key + " was not deleted; Code: " + l_Error.Code + "; " + l_Error.Message + ".";
                    return false;
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

        public static bool EmptyAwsS3Dir(string a_BucketName, string a_DirPath, ref string a_ErrorMessage)
        {
            try
            {
                AmazonS3Client l_AmazonS3Client = GetAwsS3Client();
                S3DirectoryInfo l_S3DirectoryInfo = new S3DirectoryInfo(l_AmazonS3Client, a_BucketName, a_DirPath);
                var l_Files = l_S3DirectoryInfo.GetFiles;
                foreach (var l_File in l_Files)
                    l_File.Delete();
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

        public static bool DeleteAwsS3Dir(string a_BucketName, string a_DirPath, ref string a_ErrorMessage)
        {
            try
            {
                AmazonS3Client l_AmazonS3Client = GetAwsS3Client();
                S3DirectoryInfo l_S3DirectoryInfo = new S3DirectoryInfo(l_AmazonS3Client, a_BucketName, a_DirPath);
                l_S3DirectoryInfo.Delete(true);
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
        }

        private static AmazonS3Client GetAwsS3Client()
        {
            string l_AccessKeyID = ConfigurationManager.AppSettings("AwsS3AccessKeyID");
            string l_SecretAccessKey = ConfigurationManager.AppSettings("AwsS3SecretAccessKey");
            string l_Region = ConfigurationManager.AppSettings("AwsS3Region");

            AWSCredentials l_AWSCredentials = new BasicAWSCredentials(l_AccessKeyID, l_SecretAccessKey);
            AmazonS3Client l_AmazonS3Client = new AmazonS3Client(l_AWSCredentials, AWS_S3_REGION);

            return l_AmazonS3Client;
        }

        public static void GetAwsS3FilePublicUrl(object a_BucketName, object a_FilePath)
        {
            return "http://" + a_BucketName + ".s3-website-us-east-1.amazonaws.com/" + a_FilePath;
        }
    }
}