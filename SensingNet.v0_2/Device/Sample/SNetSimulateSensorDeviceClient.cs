using CToolkit.v1_0;
using CToolkit.v1_0.Logging;
using CToolkit.v1_0.Net;
using CToolkit.v1_0.Protocol;
using CToolkit.v1_0.Secs;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra.Double;
using SensingNet.v0_2.Device;
using SensingNet.v0_2.Protocol;
using SensingNet.v0_2.Signal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SensingNet.v0_2.Device.Sample
{
    public class SNetSimulateSensorDeviceClient : IDisposable
    {
        SNetDvcSensorHandler client;
        public volatile bool IsSendRequest = false;

        ~SNetSimulateSensorDeviceClient() { this.Dispose(false); }

        public void RunAsyn()
        {

            var signalConfigs = new List<SNetSignalCfg>();
            signalConfigs.Add(new SNetSignalCfg() { Svid = 0 });

            this.client = new SNetDvcSensorHandler();
            this.client.Config = new SNetDvcSensorCfg()
            {
                DeviceUid = null,
                DeviceName = "Test",
                IsActivelyConnect = false,
                IsActivelyTx = false,
                LocalUri = null,
                ProtoConnect = SNetEnumProtoConnect.Tcp,
                ProtoFormat = SNetEnumProtoFormat.SNetCmd,
                ProtoSession = SNetEnumProtoSession.SNetCmd,
                RemoteUri = "127.0.0.1:5003",
                SerialPortConfig = null,
                SignalTran = SNetEnumSignalTran.SNetCmd,
                TimeoutResponse = 5000,
                TxInterval = 1000,
                SignalCfgList = signalConfigs,
            };




            this.client.CfInit();
            this.client.CfLoad();

            this.client.evtSignalCapture += (ss, ee) =>
            {
                var sb = new StringBuilder();
                sb.AppendFormat("Count= {0} ; Data= ", ee.CalibrateData.Count);
                foreach (var data in ee.CalibrateData)
                {
                    sb.AppendFormat("{0}, ", data);
                }
                CtkLog.InfoNs(this, sb.ToString());
            };
            this.client.ProtoConn.evtDataReceive += (ss, ee) =>
            {
                var buffer = ee.TrxMessage.As<CtkProtocolBufferMessage>();
                if (buffer == null) return;
                CtkLog.InfoNs(this, buffer.GetString());
            };


            this.client.CfRunAsyn();
        }




        public void Command(string cmd)
        {
                switch (cmd)
                {
                    case "send":
                        this.Send();
                        break;
                    case "state":
                       CtkLog.InfoNs(this,this.CmdState());
                        break;
                }
        }

        string CmdState()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("Message Count={0}\n", this.client.ProtoFormat.Count());
            return sb.ToString();
        }


        public void Send()
        {

            this.client.ProtoConn.WriteMsg("cmd\n");

        }


        public void Stop()
        {
            if (this.client != null)
            {
                using (this.client)
                {
                    this.client.CfIsRunning = false;
                    this.client.CfUnLoad();
                    this.client.CfFree();
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
