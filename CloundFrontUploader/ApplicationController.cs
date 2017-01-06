using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace CloundFrontUploader
{
    public class ApplicationController
    {
        private System.Threading.Thread _pollThread = null;
        private UploadManager _uploadManager = null;
        private System.Threading.Thread _uploadThread = null;

        private bool _isIamPolling = false;
        public ApplicationController()
        {
            this.LoadConfig();
        }
        public void StartService()
        {
            SharedClass.HasStopSignal = false;
            SharedClass.IsServiceCleaned = false;
            this._uploadManager = new UploadManager();
            
            this._uploadThread = new System.Threading.Thread(new System.Threading.ThreadStart(this._uploadManager.Start));
            this._uploadThread.Name = "Upload";
            this._uploadThread.Start();

            this._pollThread = new System.Threading.Thread(new System.Threading.ThreadStart(this.StartPolling));
            this._pollThread.Name = "Poller";
            this._pollThread.Start();
        }
        public void StopService()
        {
            while (this._isIamPolling)
            {
                SharedClass.Logger.Info("DbPoller Running. ThreadState : " + this._pollThread.ThreadState.ToString());
                if (this._pollThread.ThreadState == System.Threading.ThreadState.WaitSleepJoin)
                    this._pollThread.Interrupt();
                System.Threading.Thread.Sleep(2000);
            }
            while (this._uploadManager.IsRunning)
            {
                SharedClass.Logger.Info("UploadManager Running. ThreadState : " + this._uploadThread.ThreadState.ToString());
                if (this._uploadThread.ThreadState == System.Threading.ThreadState.WaitSleepJoin)
                    this._uploadThread.Interrupt();
                System.Threading.Thread.Sleep(2000);
            }
            SharedClass.IsServiceCleaned = true;
        }
        private void StartPolling()
        {
            SharedClass.Logger.Info("Initializing Objects");
            SqlConnection sqlCon = null;
            SqlCommand sqlCmd = null;
            SqlDataAdapter da = null;
            DataSet ds = null;
            int threadSleepTimeInSeconds = 15;
            long lastSlno = 0;
            bool recordsExist = false;

            sqlCon = new SqlConnection(SharedClass.ConnectionString);
            sqlCmd = new SqlCommand(StoredProcedures.GET_OBJECTS_TO_UPLOAD_TO_CLOUD, sqlCon);
            sqlCmd.CommandType = CommandType.StoredProcedure;
            SharedClass.Logger.Info("Started");
            this._isIamPolling = true;
            while (!SharedClass.HasStopSignal)
            {
                recordsExist = false;
                try
                {
                    sqlCmd.Parameters.Clear();
                    sqlCmd.Parameters.Add(StoredProcedureParameters.LAST_SLNO, SqlDbType.BigInt).Value = lastSlno;
                    da = new SqlDataAdapter(sqlCmd);
                    ds = new DataSet();
                    da.Fill(ds);
                    if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                    {
                        SharedClass.Logger.Info("Received " + ds.Tables[0].Rows.Count.ToString() + " New Objects To Upload");
                        recordsExist = true;
                        threadSleepTimeInSeconds = 15;
                        foreach (DataRow uploadObjectRow in ds.Tables[0].Rows)
                        {
                            try
                            {
                                this._uploadManager.EnQueue(uploadObjectRow.ToUploadObject());
                                lastSlno = uploadObjectRow["Slno"].ToLong();
                            }
                            catch (Exception e)
                            {
                                SharedClass.Logger.Error("Exception While Processing Rows. " + e.ToString());
                                uploadObjectRow.Dump();
                            }
                        }
                    }
                    if (!recordsExist)
                    {
                        threadSleepTimeInSeconds = threadSleepTimeInSeconds * 2;
                        if (threadSleepTimeInSeconds > 480)
                        {
                            threadSleepTimeInSeconds = 15;
                            SharedClass.Logger.Info("Sleep time RESET to DEFAULT");
                        }
                    }
                    try
                    {
                        System.Threading.Thread.Sleep(threadSleepTimeInSeconds * 1000);
                    }
                    catch (System.Threading.ThreadAbortException e)
                    { }
                    catch (System.Threading.ThreadInterruptedException e)
                    { }
                }
                catch (Exception e)
                {
                    SharedClass.Logger.Error("Exception in poller. " + e.ToString());
                }
                finally
                {
                    da = null;
                    ds = null;
                }
            }
            this._isIamPolling = false;
            SharedClass.Logger.Info("Exit");
        }
        private void LoadConfig()
        {
            SharedClass.InitializeLogger();
            SharedClass.Logger.Info("Loading Config into memory");
            SharedClass.ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            SharedClass.BucketName = System.Configuration.ConfigurationManager.AppSettings["BucketName"];
            SharedClass.PhysicalDirectory = System.Configuration.ConfigurationManager.AppSettings["PhysicalDirectory"];
        }
    }
}
