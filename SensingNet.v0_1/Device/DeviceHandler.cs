using CToolkit;
using CToolkit.Net;
using SensingNet.v0_1.Protocol;
using SensingNet.v0_1.Signal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SensingNet.v0_1.Device
{
    public class DeviceHandler : CToolkit.IContextFlowRun, IDisposable
    {
        public DeviceCfg config;
        public ProtoConnTcp protoConn;//連線方式
        public IProtoBase protoBase;//Protocol
        public ISignalBase signalBase;//解譯
        public EnumHandlerStatus status = EnumHandlerStatus.None;
        public Thread threadCommProc;
        AutoResetEvent areMsg = new AutoResetEvent(false);

        ~DeviceHandler() { this.Dispose(false); }


        public int CfInit()
        {
            if (this.config == null) throw new SensingNetException("沒有設定參數");

            var localIpAddr = CToolkit.CtkUtil.GetLikelyFirst127Ip(this.config.LocalIp, this.config.RemoteIp);
            var localEndPoint = new IPEndPoint(localIpAddr, this.config.LocalPort);
            var remoteEndPoint = new IPEndPoint(IPAddress.Parse(this.config.RemoteIp), this.config.RemotePort);

            this.protoConn = new ProtoConnTcp(localEndPoint, remoteEndPoint, this.config.IsActivelyConnect);
            this.protoConn.evtFirstConnect += ProtoConn_evtFirstConnect;
            this.protoConn.evtFailConnect += ProtoConn_evtFailConnect;
            this.protoConn.evtDisconnect += ProtoConn_evtDisconnect;
            this.protoConn.evtDataReceive += ProtoConn_evtDataReceive;

            return 0;
        }


        public int CfLoad()
        {

            return 0;
        }
        public int CfExec()
        {
            this.protoConn.ConnectIfNo();

            return 0;
        }
        public int CfRun()
        {
            throw new NotImplementedException("此方法不實作重複執行, 請使用CfExec");
        }
        public int CfUnLoad()
        {
            this.protoConn.Disconnect();
            //this.evtCapture = null;
            return 0;
        }
        public int CfFree()
        {
            return 0;

        }




        void RestartCommunicationProcess()
        {
            if (this.threadCommProc != null)
                this.threadCommProc.Abort();

            this.threadCommProc = new Thread(new ThreadStart(CommunicationProcess));
            this.threadCommProc.Start();
        }

        void CommunicationProcess()
        {
            var timestampStart = 0.0;
            var timestampEnd = 0.0;
            var timestampDiff = 0.0;

            var prevAckTime = DateTime.Now;


            while (!disposed)
            {

                try
                {

                    if (!this.protoConn.IsConnected) { Thread.Sleep(1000); continue; }
                    if (!this.protoConn.IsTcpClientConnected) { Thread.Sleep(1000); continue; }

                    if (this.protoBase.HasMessage())
                        SignalHandle();

                    //收到資料就往下走
                    this.areMsg.WaitOne(this.config.TimeoutResponse);

                    NetworkStream stream = this.protoConn.tcpClient.GetStream();
                    if (this.config.IsActivelyTx)
                    {
                        this.protoBase.WriteMsgDataAck(stream);
                    }
                    else
                    {
                        //等待下次要求資料的間隔
                        var now = DateTime.Now;
                        while ((now - prevAckTime).TotalMilliseconds < this.config.TxInterval) now = DateTime.Now;
                        prevAckTime = now;

                        this.protoBase.WriteMsgDataReq(stream);
                    }



                }
                catch (Exception ex) { LoggerAssembly.Write(ex); }
            }
        }


        void SignalHandle()
        {
            while (this.protoBase.HasMessage())
            {
                object msg = null;
                if (!this.protoBase.TryDequeueMsg(out msg)) return;

                var eaSignal = this.signalBase.AnalysisSignal(msg);
                eaSignal.CalibrateData = new List<double>();
                var SignalCfg = this.config.SignalCfgList.FirstOrDefault(x => x.DeviceSvid == eaSignal.DeviceSvid);
                if (SignalCfg == null) continue;
                for (int idx = 0; idx < eaSignal.Data.Count; idx++)
                {
                    var signal = eaSignal.Data[idx];
                    //var signal = d / (Math.Pow(2, 23) - 1) * 5; //轉回電壓
                    signal = signal * SignalCfg.CalibrateSysScale + SignalCfg.CalibrateSysOffset;//轉成System值
                    eaSignal.CalibrateData.Add(signal * SignalCfg.CalibrateUserScale + SignalCfg.CalibrateUserOffset);//轉入User Define
                }

                eaSignal.RcvDateTime = DateTime.Now;
                this.OnSignalCapture(eaSignal);
            }
        }

        #region Event Implement

        private void ProtoConn_evtDataReceive(object sender, CtkNonStopTcpStateEventArgs e)
        {
            var ea = e as CtkNonStopTcpStateEventArgs;
            this.protoBase.ReceiveBytes(e.buffer, e.offset, e.length);
            this.areMsg.Set();
        }

        private void ProtoConn_evtDisconnect(object sender, CtkNonStopTcpStateEventArgs e)
        {
        }

        private void ProtoConn_evtFailConnect(object sender, CtkNonStopTcpStateEventArgs e)
        {
        }

        private void ProtoConn_evtFirstConnect(object sender, CtkNonStopTcpStateEventArgs e)
        {
        }

        #endregion

        #region Event
        public event EventHandler<SignalEventArgs> evtSignalCapture;
        void OnSignalCapture(SignalEventArgs e)
        {
            if (evtSignalCapture == null) return;
            this.evtSignalCapture(this, e);
        }
        #endregion




        #region IDisposable
        // Flag: Has Dispose already been called?
        bool disposed = false;

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
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



        void DisposeManaged()
        {
            this.threadCommProc.Abort();
        }

        void DisposeUnmanaged()
        {

        }

        void DisposeSelf()
        {
            EventUtil.RemoveEventHandlersFrom(delegate (Delegate dlgt) { return true; }, this);
        }

        #endregion

    }
}
