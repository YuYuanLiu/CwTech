using CToolkit.v0_1;
using CToolkit.v0_1.Logging;
using CToolkit.v0_1.Net;
using CToolkit.v0_1.Secs;
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
    public class SNetSimulateSensorDeviceClient : IDisposable
    {
        SNetSensorDeviceHandler client;
        public volatile bool IsSendRequest = false;

        ~SNetSimulateSensorDeviceClient() { this.Dispose(false); }

        public void RunAsyn()
        {

            var signalConfigs = new List<SNetSignalCfg>();
            signalConfigs.Add(new SNetSignalCfg() { Svid = 0 });

            this.client = new SNetSensorDeviceHandler();
            this.client.Config = new SNetSensorDeviceCfg()
            {
                DeviceId = 0,
                DeviceName = "Test",
                IsActivelyConnect = false,
                IsActivelyTx = false,
                LocalIp = null,
                LocalPort = 0,
                ProtoConnect = Protocol.SNetEnumProtoConnect.Tcp,
                ProtoFormat = Protocol.SNetEnumProtoFormat.SNetCmd,
                ProtoSession = Protocol.SNetEnumProtoSession.SNetCmd,
                RemoteIp = "127.0.0.1",
                RemotePort = 5003,
                SerialPortConfig = null,
                SignalTran = Signal.SNetEnumSignalTran.SNetCmd,
                TimeoutResponse = 5000,
                TxInterval = 1000,
                SignalCfgList = signalConfigs,
            };


            this.client.evtSignalCapture += (ss, ee) =>
            {
                var sb = new StringBuilder();
                sb.AppendFormat("Data Count={0}", ee.CalibrateData.Count);
                Write(sb.ToString());
            };

            this.client.CfInit();
            this.client.CfLoad();
            this.client.CfRunAsyn();
        }


        public void Write(string msg, params object[] obj)
        {
            Console.WriteLine();
            Console.WriteLine(msg, obj);
            Console.Write(">");
        }

        public void CommandLine()
        {
            var cmd = "";
            do
            {
                Write(this.GetType().Name);
                cmd = Console.ReadLine();

                switch (cmd)
                {
                    case "send":
                        this.Send();
                        break;
                    case "state":
                        this.Write(this.CmdState());
                        break;
                }


            } while (string.Compare(cmd, "exit", true) != 0);

            this.Stop();

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
                this.DisposeManaged();
            }

            // Free any unmanaged objects here.
            //
            this.DisposeUnmanaged();

            this.DisposeSelf();

            disposed = true;
        }



        protected virtual void DisposeManaged()
        {
        }

        protected virtual void DisposeSelf()
        {
            this.Stop();
        }

        protected virtual void DisposeUnmanaged()
        {

        }
        #endregion


    }
}
