using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloundFrontUploader
{
    class UploadManager
    {
        private Queue<UploadObject> _uploadQueue = new Queue<UploadObject>();
        private bool _isIamRunning = false;
        public void Start()
        {
            SharedClass.Logger.Info("Started");
            bool bucketExist = false;
            bool directoryExist = false;
            this._isIamRunning = true;
            while (!SharedClass.HasStopSignal)
            {
                bucketExist = false;
                directoryExist = false;
                if (this.QueueCount() > 0)
                {
                    UploadObject uploadObject = this.DeQueue();
                    if (uploadObject == null)
                    {
                        SharedClass.Logger.Error("uploadObject is null in In-Memory Queue");
                        System.Threading.Thread.Sleep(2000);
                        continue;
                    }
                    try
                    {
                        SharedClass.Logger.Info("Started Processing " + uploadObject.ToReadable());                        
                        if (!uploadObject.BucketName.BucketExist())
                            bucketExist = uploadObject.BucketName.CreateBucket();
                        else
                            bucketExist = true;
                        if (!bucketExist)
                        {
                            SharedClass.Logger.Error("Bucket Not Created. Terminating Uploading Process Of Object " + uploadObject.ToReadable());
                            continue;
                        }
                        if (uploadObject.Path.Contains("/"))
                        {
                            if (!uploadObject.Path.Substring(0, uploadObject.Path.LastIndexOf("/")).DirectiryExist(uploadObject.BucketName))
                                directoryExist = uploadObject.Path.Substring(0, uploadObject.Path.LastIndexOf("/")).CreateDirectory(uploadObject.BucketName);
                            else
                                directoryExist = true;
                        }
                        else
                            directoryExist = true;
                        if (!directoryExist)
                        {
                            SharedClass.Logger.Error("Directory Not Created. Terminating Uploading Process Of " + uploadObject.Path + " Into Bucket " + uploadObject.BucketName);
                            continue;
                        }
                        if (uploadObject.Path.UploadToCloud(uploadObject.BucketName))
                            uploadObject.UpdateStatus(true);
                        else
                            uploadObject.UpdateStatus(false);
                    }
                    catch (Exception e)
                    {
                        SharedClass.Logger.Error("Exception In UploadManager While Loop. " + e.ToString() + " ======= Object : " + uploadObject.ToReadable());
                    }
                }
                else
                {
                    try
                    { System.Threading.Thread.Sleep(5000); }
                    catch (System.Threading.ThreadInterruptedException e)
                    { }
                    catch (System.Threading.ThreadAbortException e)
                    { }
                }
            }
            SharedClass.Logger.Info("Exit");
            this._isIamRunning = false;
        }
        private long QueueCount()
        {
            lock (this._uploadQueue)
            {
                return this._uploadQueue.Count;
            }
        }
        public void EnQueue(UploadObject uploadObject)
        {
            SharedClass.Logger.Info("EnQueing ==> " + uploadObject.ToReadable());
            lock (this._uploadQueue)
            {
                this._uploadQueue.Enqueue(uploadObject);
            }
        }
        private UploadObject DeQueue()
        {
            lock (this._uploadQueue)
            {
                return this._uploadQueue.Dequeue();
            }
        }
        public bool IsRunning
        {
            get { return this._isIamRunning; }
        }
    }
}
