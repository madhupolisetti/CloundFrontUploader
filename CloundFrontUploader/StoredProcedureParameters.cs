using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloundFrontUploader
{
    public static class StoredProcedureParameters
    {
        public const string LAST_SLNO = "@LastSlno";
        public const string QUEUE_TABLE_SLNO = "@QueueTableSlno";
        public const string IS_UPLOAD_SUCCESS = "@IsUploadSuccess";
        public const string SUCCESS = "@Success";
        public const string MESSAGE = "@Message";
    }
}
