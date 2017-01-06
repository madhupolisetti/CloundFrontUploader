using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace CloundFrontUploader
{
    public partial class UploadService : ServiceBase
    {
        ApplicationController _applicationController = null;
        System.Threading.Thread _appThread = null;
        public UploadService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _applicationController = new ApplicationController();
            _appThread = new System.Threading.Thread(new System.Threading.ThreadStart(_applicationController.StartService));
            _appThread.Start();
        }

        protected override void OnStop()
        {
            SharedClass.Logger.Info("Stop Signal Received");
            SharedClass.HasStopSignal = false;
        }
    }
}
