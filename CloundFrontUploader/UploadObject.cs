using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace CloundFrontUploader
{
    public class UploadObject
    {
        private long _tableSlno = 0;
        private long _queueSlno = 0;
        //private ImageCategory _category = ImageCategory.UNKNOWN;
        private string _tableName = string.Empty;
        private string _tableSlnoColumnName = string.Empty;
        private string _bucketName = string.Empty;        
        private string _path = string.Empty;
        private string _columnName = string.Empty;
        public long TableSlno
        {
            get { return this._tableSlno; }
            set { this._tableSlno = value; }
        }
        public long QueueSlno
        {
            get { return this._queueSlno; }
            set { this._queueSlno = value; }
        }
        //public ImageCategory Category
        //{
        //    get { return this._category; }
        //    set { this._category = value; }
        //}
        public string TableName
        {
            get { return this._tableName; }
            set { this._tableName = value; }
        }
        public string BucketName
        {
            get { return this._bucketName; }
            set { this._bucketName = value; }            
        }
        public string Path
        {
            get { return this._path; }
            set { this._path = value; }
        }
        public string ColumnName
        {
            get { return this._columnName; }
            set { this._columnName = value; }
        }
        public string TableSlnoColumnName
        {
            get { return this._tableSlnoColumnName; }
            set { this._tableSlnoColumnName = value; }
        }
        public void UpdateStatus(bool isSuccess)
        {
            SqlConnection sqlCon = null;
            SqlCommand sqlCmd = null;
            try
            {
                sqlCon = new SqlConnection(SharedClass.ConnectionString);
                sqlCmd = new SqlCommand(StoredProcedures.UPDATE_UPLOAD_OBJECT_STATUS, sqlCon);
                sqlCmd.CommandType = CommandType.StoredProcedure;
                sqlCmd.Parameters.Add(StoredProcedureParameters.QUEUE_TABLE_SLNO, SqlDbType.BigInt).Value = this._queueSlno;
                sqlCmd.Parameters.Add(StoredProcedureParameters.IS_UPLOAD_SUCCESS, SqlDbType.Bit).Value = isSuccess;
                sqlCmd.Parameters.Add(StoredProcedureParameters.SUCCESS, SqlDbType.Bit).Direction = ParameterDirection.Output;
                sqlCmd.Parameters.Add(StoredProcedureParameters.MESSAGE, SqlDbType.VarChar, 1000).Direction = ParameterDirection.Output;
                sqlCon.Open();
                sqlCmd.ExecuteNonQuery();
                SharedClass.Logger.Info("Upload Status Update Success. QueueTableSlno : " + this._queueSlno.ToString() + ", IsUploadSuccess : " + isSuccess);
            }
            catch (Exception e)
            {
                SharedClass.Logger.Error("Exception While Updating Status. QueueTableSlno " + this._queueSlno.ToString());
            }
            finally
            {
                if (sqlCon != null && sqlCon.State == ConnectionState.Open)
                    sqlCon.Close();
                sqlCon = null;
                sqlCmd = null;
            }
        }
    }
}
