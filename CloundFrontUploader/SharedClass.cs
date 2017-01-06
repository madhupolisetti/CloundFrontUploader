using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace CloundFrontUploader
{
    public static class SharedClass
    {
        private static ILog _logger = null;
        private static bool _hasStopSignal = true;
        private static string _connectionString = null;
        private static bool _isServiceCleaned = true;
        private static string _bucketName = string.Empty;
        private static string _physicalDirectory = string.Empty;
        public static Amazon.S3.IAmazonS3 S3CLIENT = Amazon.AWSClientFactory.CreateAmazonS3Client(region: Amazon.RegionEndpoint.APSoutheast1);
        public static void InitializeLogger()
        {
            GlobalContext.Properties["LogName"] = DateTime.Now.ToString("yyyyMMdd");
            log4net.Config.XmlConfigurator.Configure();
            _logger = LogManager.GetLogger("Log");
            _logger.Info("Log Initialized");
        }
        public static ILog Logger
        {
            get { return _logger; }
        }
        public static bool HasStopSignal
        {
            get { return _hasStopSignal; }
            set { _hasStopSignal = value; }
        }
        public static string ConnectionString
        {
            get { return _connectionString; }
            set { _connectionString = value; }
        }
        public static bool IsServiceCleaned 
        {
            get { return _isServiceCleaned; }
            set { _isServiceCleaned = value; }
        }
        public static string BucketName
        {
            get { return _bucketName; }
            set { _bucketName = value; }
        }
        public static string PhysicalDirectory
        {
            get { if (!_physicalDirectory.EndsWith("\\")) { _physicalDirectory = _physicalDirectory + "\\"; } return _physicalDirectory; }
            set { _physicalDirectory = value; }
        }
    }
}
