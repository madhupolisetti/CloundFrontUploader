using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

namespace CloundFrontUploader
{
    public static class ExtensionMethods
    {
        public static long ToLong(this object obj)
        {
            return Convert.ToInt64(obj);
        }
        public static bool CanUpload(this object obj)
        {
            return !obj.Equals(DBNull.Value) && obj.ToString().Replace(" ", "").Length > 0;
        }
        //public static ImageCategory GetCategory(this byte input)
        //{ 
        //    ImageCategory category;
        //    switch (input)
        //    {
        //        case 1:
        //            category = ImageCategory.MOVIE_IMAGES;
        //            break;
        //        case 2:
        //            category = ImageCategory.MOVIE_QR_CODES;
        //            break;
        //        case 3:
        //            category = ImageCategory.MOVIE_CAST;
        //            break;
        //        case 4:
        //            category = ImageCategory.EVENT_IMAGES;
        //            break;
        //        case 5:
        //            category = ImageCategory.EVENT_QR_CODES;
        //            break;
        //        default:
        //            category = ImageCategory.UNKNOWN;
        //            break;
        //    }
        //    return category;
        //}
        //public static byte GetByte(this ImageCategory category)
        //{
        //    byte value = 0;
        //    switch (category)
        //    { 
        //        case ImageCategory.MOVIE_IMAGES:
        //            value = 1;
        //            break;
        //        case ImageCategory.MOVIE_QR_CODES:
        //            value = 2;
        //            break;
        //        case ImageCategory.MOVIE_CAST:
        //            value = 3;
        //            break;
        //        case ImageCategory.EVENT_IMAGES:
        //            value = 4;
        //            break;
        //        case ImageCategory.EVENT_QR_CODES:
        //            value = 5;
        //            break;
        //        case ImageCategory.UNKNOWN:
        //            value = 0;
        //            break;
        //        default:
        //            value = 0;
        //            break;
        //    }
        //    return value;
        //}
        public static bool BucketExist(this string bucketName)
        {
            bool exist = false;
            ListBucketsRequest request = null;
            ListBucketsResponse response = null;
            try
            {
                request = new ListBucketsRequest();
                response = SharedClass.S3CLIENT.ListBuckets(request);
                if (response.Buckets.Count > 0)
                    foreach (S3Bucket bucket in response.Buckets)
                        if (bucket.BucketName == bucketName)
                        {
                            exist = true;
                            break;
                        }
            }
            catch (Exception e)
            {
                exist = false;
                SharedClass.Logger.Error("Exception while listing buckets. " + e.ToString());
            }
            finally
            {
                request = null;
                response = null;
            }
            return exist;
        }
        public static bool CreateBucket(this string bucketName)
        {
            SharedClass.Logger.Info("Creating Bucket " + bucketName);
            bool isCreated = true;
            PutBucketRequest request = null;
            PutBucketResponse response = null;
            try
            {
                request = new PutBucketRequest();
                request.BucketName = bucketName;
                request.BucketRegion = S3Region.APS1;
                request.CannedACL = S3CannedACL.PublicRead;
                response = SharedClass.S3CLIENT.PutBucket(request);
                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    isCreated = true;
                    SharedClass.Logger.Info("Bucket Created Successfully");
                }
                else
                {
                    isCreated = false;
                    SharedClass.Logger.Error("Error While Creating Bucket. StatusCode :" + response.HttpStatusCode.ToString());
                }
            }
            catch (Exception e)
            {
                SharedClass.Logger.Error("Exception while creating bucket {" + bucketName + "}. " + e.ToString());
                return false;
            }
            finally
            {
                request = null;
                response = null;
            }
            return isCreated;
        }
        public static bool DirectiryExist(this string directory, string bucketName)
        {
            if (!directory.EndsWith("/"))
                directory = directory + "/";
            if (directory.StartsWith("/"))
                directory = directory.Substring(1, directory.Length - 1);
            bool isExist = false;
            ListObjectsRequest request = null;
            ListObjectsResponse response = null;
            try
            {
                request = new ListObjectsRequest();
                request.BucketName = bucketName;
                request.Prefix = directory;
                response = SharedClass.S3CLIENT.ListObjects(request);
                foreach (S3Object obj in response.S3Objects)
                {
                    if (obj.Key == directory)
                    {
                        isExist = true;
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                SharedClass.Logger.Error("Exception while listing Directories. " + e.ToString());
                isExist = false;
            }
            finally
            {
                request = null;
                response = null;
            }
            return isExist;
        }
        public static bool CreateDirectory(this string directory, string bucketName)
        {
            SharedClass.Logger.Info("Creating Directory " + directory + " In Bucket " + bucketName);
            if (!directory.EndsWith("/"))
            {
                directory = directory + "/";
                SharedClass.Logger.Info("Appending \"/\" at the End");
            }
            if (directory.StartsWith("/"))
                directory = directory.Substring(1, directory.Length - 1);
            bool isCreated = false;
            PutObjectRequest request = null;
            PutObjectResponse response = null;
            try
            {
                request = new PutObjectRequest();
                request.BucketName = bucketName;
                request.CannedACL = S3CannedACL.PublicRead;
                request.Key = directory;
                request.StorageClass = S3StorageClass.Standard;
                request.ContentBody = string.Empty;
                response = SharedClass.S3CLIENT.PutObject(request);
                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    isCreated = true;
                    SharedClass.Logger.Info("Directory Created");
                }
                else
                {
                    SharedClass.Logger.Error("Error while Creating Directory. StatusCode " + response.HttpStatusCode.ToString());
                    isCreated = false;
                }
            }
            catch (Exception e)
            {
                SharedClass.Logger.Error("Exception while creating Directory " + directory + ". " + e.ToString());
            }
            finally
            {
                request = null;
                response = null;
            }
            return isCreated;
        }
        public static bool UploadToCloud(this string fullFilePath, string bucketName)
        {
            SharedClass.Logger.Info("Creating Object " + fullFilePath + " In Bucket " + bucketName);
            if (fullFilePath.StartsWith("/"))
                fullFilePath = fullFilePath.Substring(1, fullFilePath.Length - 1);
            bool isCreated = false;
            PutObjectRequest request = null;
            PutObjectResponse response = null;
            try
            {
                request = new PutObjectRequest();
                request.BucketName = bucketName;
                request.Key = fullFilePath;
                request.FilePath = SharedClass.PhysicalDirectory + fullFilePath.Replace("/", "\\");
                request.ContentType = fullFilePath.MimeType();
                request.CannedACL = S3CannedACL.PublicRead;
                request.StorageClass = S3StorageClass.Standard;
                response = SharedClass.S3CLIENT.PutObject(request);
                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    isCreated = true;
                    SharedClass.Logger.Info("Object Created Successfully");
                }
                else
                {
                    isCreated = false;
                    SharedClass.Logger.Error("Error While Creating Object. StatusCode : " + response.HttpStatusCode);
                }
            }
            catch (Exception e)
            {
                SharedClass.Logger.Error("Exception While Creating Object. " + e.ToString());
                isCreated = false;
            }
            finally
            {
                request = null;
                response = null;
            }
            return isCreated;
        }
        public static UploadObject ToUploadObject(this System.Data.DataRow row)
        {
            UploadObject uploadObject = new UploadObject();
            uploadObject.QueueSlno = row["Slno"].ToLong();
            uploadObject.TableSlno = row["TableSlno"].ToLong();
            uploadObject.TableSlnoColumnName = row["TableSlnoColumnName"].ToString();
            uploadObject.TableName = row["TableName"].ToString();
            uploadObject.ColumnName = row["ColumnName"].ToString();
            uploadObject.Path = row["FilePath"].ToString();
            uploadObject.BucketName = SharedClass.BucketName;
            return uploadObject;
        }
        public static string MimeType(this string fileName)
        {
            string mimeType = string.Empty;
            switch (fileName.Split(new char['.']).Last().ToLower())
            { 
                case FileExtensions.BM:
                    mimeType = MimeTypes.IMAGE_BMP;
                    break;
                case FileExtensions.BMP:
                    mimeType = MimeTypes.IMAGE_BMP;
                    break;
                case FileExtensions.ICO:
                    mimeType = MimeTypes.IMAGE_X_ICON;
                    break;
                case FileExtensions.JPE:
                    mimeType = MimeTypes.IMAGE_JPEG;
                    break;
                case FileExtensions.JPG:
                    mimeType = MimeTypes.IMAGE_JPEG;
                    break;
                case FileExtensions.JPEG:
                    mimeType = MimeTypes.IMAGE_JPEG;
                    break;
                case FileExtensions.JFIF:
                    mimeType = MimeTypes.IMAGE_JPEG;
                    break;
                case FileExtensions.PNG:
                    mimeType = MimeTypes.IMAGE_PNG;
                    break;
                case FileExtensions.GIF:
                    mimeType = MimeTypes.IMAGE_GIF;
                    break;
                default:
                    mimeType = MimeTypes.TEXT_PLAIN;
                    break;
            }
            return mimeType;
        }
        public static void Dump(this System.Data.DataRow row)
        {
            foreach (System.Data.DataColumn column in row.Table.Columns)
            {
                SharedClass.Logger.Info(column.ColumnName + " : " + (row[column].Equals(DBNull.Value) ? "NULL" : row[column].ToString()));
            }
        }
        public static string ToReadable(this UploadObject uploadObject)
        {
            return " QueueTableSlno : " + uploadObject.QueueSlno.ToString() + ", Path : " + uploadObject.Path + ", TableName : " + uploadObject.TableName + ", TableSlno : " + uploadObject.TableSlno.ToString() + ", TableSlnoColumnName : " + uploadObject.TableSlnoColumnName + ", ColumnName : " + uploadObject.ColumnName;
        }
    }
}
