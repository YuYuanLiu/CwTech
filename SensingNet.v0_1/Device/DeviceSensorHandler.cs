using CToolkit;
using CToolkit.Net;
using CToolkit.Protocol;
using CToolkit.v0_1;
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
    public class DeviceSensorHandler : IContextFlowRun, IDisposable
    {
        public DeviceSensorCfg Config;
        public IProtoConnectBase ProtoConn;//連線方式
        public IProtoFormatBase ProtoFormat;//Protocol
        public ISignalTranBase SignalTran;//解譯
        public EnumHandlerStatus status = EnumHandlerStatus.None;
        Task<int> runTask;
        AutoResetEvent areMsg = new AutoResetEvent(false);
        DateTime prevAckTime = DateTime.Now;

        public DeviceSensorHandler() { }
        ~DeviceSensorHandler() { this.Dispose(false); }


        int RealExec()
        {
            try
            {
                if (!this.ProtoConn.IsRemoteConnected) { Thread.Sleep(1000); return 0; }

                if (this.Config.IsActivelyTx)
                {
                    this.ProtoFormat.WriteMsgDataAck(this.ProtoConn);
                }
                else
                {
                    //等待下次要求資料的間隔
                    var now = DateTime.Now;
                    if (this.Config.TxInterval > 0)
                        while ((now - prevAckTime).TotalMilliseconds < this.Config.TxInterval) now = DateTime.Now;
                    prevAckTime = now;

                    this.ProtoFormat.WriteMsgDataReq(this.ProtoConn);
                }


                //收到資料 或 Timeout 就往下走
                this.areMsg.WaitOne(this.Config.TimeoutResponse);


            }
            catch (Exception ex) { CtkLog.Write(ex); }
            return 0;
        }
        void SignalHandle()
        {
            while (this.ProtoFormat.HasMessage())
            {
                object msg = null;
                if (!this.ProtoFormat.TryDequeueMsg(out msg)) return;

                var eaSignal = this.SignalTran.AnalysisSignal(msg);
                eaSignal.CalibrateData = new List<double>();
                var SignalCfg = this.Config.SignalCfgList.FirstOrDefault(x => x.Svid == eaSignal.Svid);
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



        #region Event
        public event EventHandler<SignalEventArgs> evtSignalCapture;
        void OnSignalCapture(SignalEventArgs e)
        {
            if (evtSignalCapture == null) return;
            this.evtSignalCapture(this, e);
        }
        #endregion



        #region IContextFlowRun

        /// <summary>
        /// 取得是否正在執行, 可由User設定為false
        /// </summary>
        public bool CfIsRunning { get; set; }
        public int CfInit()
        {
            if (this.Config == null) throw new SensingNetException("沒有設定參數");

            var localIpAddr = NetUtil.GetSuitableIp(this.Config.LocalIp, this.Config.RemoteIp);
            var localEndPoint = new IPEndPoint(localIpAddr, this.Config.LocalPort);
            var remoteEndPoint = new IPEndPoint(IPAddress.Parse(this.Config.RemoteIp), this.Config.RemotePort);



            switch (this.Config.ProtoConnect)
            {
                case EnumDeviceProtoConnect.Tcp:
                    this.ProtoConn = new ProtoConnTcp(localEndPoint, remoteEndPoint, this.Config.IsActivelyConnect);
                    break;
                case EnumDeviceProtoConnect.Rs232:
                    this.ProtoConn = new ProtoConnRs232(this.Config.ComPort);
                    break;
                default:
                    //由使用者自己實作
                    break;
            }

            if (this.ProtoConn == null) throw new ArgumentException("ProtoConn");
            this.ProtoConn.evtDataReceive += (sender, e) =>
            {
                var ea = e as CtkNonStopTcpStateEventArgs;
                this.ProtoFormat.ReceiveBytes(ea.buffer, ea.offset, ea.length);
                this.areMsg.Set();

                if (this.ProtoFormat.HasMessage())
                    SignalHandle();
            };




            switch (this.Config.ProtoFormat)
            {
                case EnumDeviceProtoFormat.SensingNetCmd:
                    this.ProtoFormat = new ProtoFormatSensingNetCmd();
                    this.SignalTran = new SignalTranSensingNet();
                    break;
                default:
                    //由使用者自己實作
                    break;
            }
            if (this.ProtoFormat == null) throw new ArgumentException("必須指定ProtoFormat");




            return 0;
        }


        public int CfLoad()
        {
      
     

            return 0;
        }
        public int CfExec()
        {
            this.ProtoConn.ConnectIfNo();//內部會處理重複要求連線
            RealExec();
            return 0;
        }
        public int CfRun()
        {
            this.ProtoConn.NonStopConnectAsyn();

            this.CfIsRunning = true;
            while (!disposed && this.CfIsRunning)
            {
                this.RealExec();
            }
            return 0;
        }
        public int CfRunAsyn()
        {
            if (this.runTask != null)
                if (!this.runTask.Wait(100)) return 0;//正在工作

            this.runTask = Task.Factory.StartNew<int>(() => this.CfRun());
            return 0;
        }
        public int CfUnLoad()
        {
            this.CfIsRunning = false;
            if (this.ProtoConn != null)
            {
                this.ProtoConn.Disconnect();
                this.ProtoConn = null;
            }
            return 0;
        }
        public int CfFree()
        {
            this.Dispose(false);
            return 0;
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
        }

        void DisposeUnmanaged()
        {

        }

        void DisposeSelf()
        {
            if (this.runTask != null)
                this.runTask.Dispose();
            if (this.ProtoConn != null)
                this.ProtoConn.Dispose();

            EventUtil.RemoveEventHandlersFrom(delegate (Delegate dlgt) { return true; }, this);
        }

        #endregion

    }
}
