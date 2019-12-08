using CToolkit.v1_0;
using CToolkit.v1_0.Logging;
using CToolkit.v1_0.Net;
using CToolkit.v1_0.Secs;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra.Double;
using SensingNet.v0_1.Device;
using SensingNet.v0_1.Signal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SensingNet.v0_1.Device.Simulate
{
    public class SNetSimulateSensorDeviceClientVibration : IDisposable
    {
        SNetSensorDeviceHandler device;
        public volatile bool IsSendRequest = false;

        ~SNetSimulateSensorDeviceClientVibration() { this.Dispose(false); }

        public void RunAsyn()
        {

            this.device = new SNetSensorDeviceHandler();
            this.device.Config = new SNetSensorDeviceCfg()
            {
                DeviceUid = null,
                DeviceName = "Test127",
                IntervalTimeOfConnectCheck = 1000,
                IsActivelyConnect = false,
                IsActivelyTx = true,
                LocalIp = null,
                LocalPort = 0,
                ProtoConnect = Protocol.SNetEnumProtoConnect.Tcp,
                ProtoFormat = Protocol.SNetEnumProtoFormat.SNetCmd,
                ProtoSession = Protocol.SNetEnumProtoSession.SNetCmd,
                RemoteIp = "127.0.0.1",
                RemotePort = 5003,
                SerialPortConfig = null,
                SignalCfgList = new SNetSignalCfg[]
                               {
                                    new SNetSignalCfg()
                                    {
                                       Svid= 0,
                                    }
                               }.ToList(),
                SignalTran = SNetEnumSignalTran.SNetCmd,
                TimeoutResponse = 5000,
                TxInterval = 0,
                Uri = null,
            };
            this.device.evtSignalCapture += (ss, ee) =>
            {
                var sb = new StringBuilder();
                foreach (var val in ee.CalibrateData)
                {
                    sb.Append(val + ",");
                }
                CtkLog.InfoNs(this, sb.ToString());
            };

            this.device.CfInit();
            this.device.CfLoad();
            this.device.CfRunAsyn();



        }




        public void Command(string cmd)
        {
            switch (cmd)
            {
                case "send":
                    this.Send();
                    break;
            }
        }


        public void Send()
        {




        }


        public void Stop()
        {
            if (this.device != null)
            {
                using (this.device)
                {
                    this.device.CfUnLoad();
                    this.device.CfFree();
                }
            }
        }


        #region IDisposable
        // Flag: Has Dispose already been called?
        protected bool disposed = false;

        // Public implementation of Dispose pattern callable by consumers.
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
                //
            }

            // Free any unmanaged objects here.
            //

            this.DisposeSelf();

            disposed = true;
        }





        protected virtual void DisposeSelf()
        {
            this.Stop();
        }



        #endregion


    }
}
