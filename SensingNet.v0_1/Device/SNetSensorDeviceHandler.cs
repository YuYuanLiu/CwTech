using CToolkit;
using CToolkit.v0_1.Net;
using CToolkit.v0_1.Protocol;
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
using CToolkit.v0_1.Threading;

namespace SensingNet.v0_1.Device
{
    public class SNetSensorDeviceHandler : ICtkContextFlowRun, IDisposable
    {
        public SNetSensorDeviceCfg Config;
        public ISNetProtoConnectBase ProtoConn;//連線方式
        public ISNetProtoFormatBase ProtoFormat;//Protocol
        public ISNetProtoSessionBase ProtoSession;
        public ISNetSignalTranBase SignalTran;//解譯
        public SNetEnumHandlerStatus Status = SNetEnumHandlerStatus.None;

        AutoResetEvent areMsg = new AutoResetEvent(false);
        DateTime prevAckTime = DateTime.Now;
        Task<int> runTask;
        CancellationTokenSource runTaskToken = new CancellationTokenSource();
        public SNetSensorDeviceHandler() { }
        ~SNetSensorDeviceHandler() { this.Dispose(false); }


        protected virtual int RealExec()
        {
            try
            {
                if (!this.ProtoConn.IsRemoteConnected) { Thread.Sleep(1000); return 0; }

                if (this.Config.IsActivelyTx)
                {
                    var ackDataMsg = this.SignalTran.CreateMsgDataAck(this.Config.SignalCfgList);
                    if (ackDataMsg != null)
                        this.ProtoConn.WriteMsg(ackDataMsg);
                }
                else
                {
                    //等待下次要求資料的間隔
                    var now = DateTime.Now;
                    if (this.Config.TxInterval > 0)
                        while ((now - prevAckTime).TotalMilliseconds < this.Config.TxInterval) now = DateTime.Now;
                    prevAckTime = now;

                    var reqDataMsg = this.SignalTran.CreateMsgDataReq(this.Config.SignalCfgList);
                    this.ProtoConn.WriteMsg(reqDataMsg);
                }


                //收到資料 或 Timeout 就往下走
                this.areMsg.WaitOne(this.Config.TimeoutResponse);


            }
            catch (Exception ex) { CtkLog.Write(ex); }
            return 0;
        }
        protected virtual void SignalHandle()
        {
            while (this.ProtoFormat.HasMessage())
            {
                object msg = null;
                if (!this.ProtoFormat.TryDequeueMsg(out msg)) return;

                if (this.ProtoSession.ProcessSession(this.ProtoConn, msg)) continue;


                var eaSignals = this.SignalTran.AnalysisSignal(this, msg, this.Config.SignalCfgList);
                for (var idx = 0; idx < eaSignals.Count; idx++)
                {
                    var eaSignal = eaSignals[idx];

                    eaSignal.Sender = this;
                    eaSignal.CalibrateData = new List<double>();

                    if (eaSignal.Svid == null && this.Config.SignalCfgList.Count > idx)
                        eaSignal.Svid = this.Config.SignalCfgList[idx].Svid;


                    var signalCfg = this.Config.SignalCfgList.FirstOrDefault(x => x.Svid == eaSignal.Svid);
                    if (signalCfg == null) continue;
                    for (int idx_data = 0; idx_data < eaSignal.Data.Count; idx_data++)
                    {
                        var signal = eaSignal.Data[idx_data];
                        //var signal = d / (Math.Pow(2, 23) - 1) * 5; //轉回電壓
                        signal = signal * signalCfg.CalibrateSysScale + signalCfg.CalibrateSysOffset;//轉成System值
                        eaSignal.CalibrateData.Add(signal * signalCfg.CalibrateUserScale + signalCfg.CalibrateUserOffset);//轉入User Define
                    }

                    eaSignal.SignalConfig = signalCfg;
                    eaSignal.RcvDateTime = DateTime.Now;
                    this.OnSignalCapture(eaSignal);
                }
            }
        }



        #region Event
        public event EventHandler<SNetSignalEventArgs> evtSignalCapture;
        void OnSignalCapture(SNetSignalEventArgs e)
        {
            if (evtSignalCapture == null) return;
            this.evtSignalCapture(this, e);
        }
        #endregion



        #region ICtkContextFlowRun

        /// <summary>
        /// 取得是否正在執行, 可由User設定為false
        /// </summary>
        public bool CfIsRunning { get; set; }
        public virtual int CfExec()
        {
            this.ProtoConn.ConnectIfNo();//內部會處理重複要求連線
            RealExec();
            return 0;
        }

        public virtual int CfFree()
        {
            this.Dispose(false);
            return 0;
        }

        public virtual int CfInit()
        {
            if (this.Config == null) throw new SNetException("沒有設定參數");

            var localIpAddr = CtkNetUtil.GetSuitableIp(this.Config.LocalIp, this.Config.RemoteIp);
            var localEndPoint = new IPEndPoint(localIpAddr, this.Config.LocalPort);
            var remoteEndPoint = new IPEndPoint(IPAddress.Parse(this.Config.RemoteIp), this.Config.RemotePort);



            switch (this.Config.ProtoConnect)
            {
                case SNetEnumProtoConnect.Tcp:
                    this.ProtoConn = new SNetProtoConnTcp(localEndPoint, remoteEndPoint, this.Config.IsActivelyConnect);
                    break;
                case SNetEnumProtoConnect.Rs232:
                    this.ProtoConn = new SNetProtoConnRs232(this.Config.SerialPortConfig);
                    break;
                default:
                    //由使用者自己實作
                    break;
            }

            if (this.ProtoConn == null) throw new ArgumentException("ProtoConn");
            this.ProtoConn.evtDataReceive += (sender, e) =>
            {
                var ea = e as CtkProtocolBufferEventArgs;
                this.ProtoFormat.ReceiveBytes(ea.buffer, ea.offset, ea.length);
                this.areMsg.Set();

                if (this.ProtoFormat.HasMessage())
                    SignalHandle();
            };




            switch (this.Config.ProtoFormat)
            {
                case SNetEnumProtoFormat.SensingNetCmd:
                    this.ProtoFormat = new SNetProtoFormatSensingNetCmd();
                    break;
                default:
                    //由使用者自己實作
                    break;
            }
            if (this.ProtoFormat == null) throw new ArgumentException("必須指定ProtoFormat");



            switch (this.Config.ProtoSession)
            {
                case SNetEnumProtoSession.SensingNetCmd:
                    this.ProtoSession = new SNetProtoSessionSensingNetCmd();
                    break;
                case SNetEnumProtoSession.Secs:
                    this.ProtoSession = new SNetProtoSessionSecs();
                    break;
                default:
                    //由使用者自己實作
                    break;
            }
            if (this.ProtoSession == null) throw new ArgumentException("必須指定ProtoFormat");

            switch (this.Config.SignalTran)
            {
                case SNetEnumSignalTran.SensingNet:
                    this.SignalTran = new SNetSignalTranSensingNet();
                    break;
                case SNetEnumSignalTran.Secs001:
                    this.SignalTran = new SNetSignalTranSecs001();
                    break;
                default:
                    //由使用者自己實作
                    break;
            }
            if (this.ProtoSession == null) throw new ArgumentException("必須指定ProtoFormat");





            return 0;
        }
        public virtual int CfLoad()
        {



            return 0;
        }
        public virtual int CfRun()
        {
            this.ProtoConn.NonStopConnectAsyn();

            this.CfIsRunning = true;
            while (!disposed && this.CfIsRunning)
            {
                this.RealExec();
            }
            return 0;
        }
        public virtual int CfRunAsyn()
        {
            if (this.runTask != null)
                if (!this.runTask.Wait(100)) return 0;//正在工作

            this.runTask = Task.Factory.StartNew<int>(() => this.CfRun());
            return 0;
        }
        public virtual int CfUnLoad()
        {
            this.CfIsRunning = false;
            if (this.ProtoConn != null)
            {
                this.ProtoConn.Disconnect();
                this.ProtoConn = null;
            }
            return 0;
        }

        #endregion



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
        protected virtual void DisposeUnmanaged()
        {

        }
        protected virtual void DisposeSelf()
        {
            this.CfIsRunning = false;

            if (this.runTask != null)
            {
                SpinWait.SpinUntil(() => this.runTask.IsCompleted || this.runTask.IsFaulted || this.runTask.IsCanceled, 3000);
                this.runTask.Dispose();
            }
            if (this.ProtoConn != null)
                this.ProtoConn.Dispose();

            CtkEventUtil.RemoveEventHandlersFromOwningByFilter(this, (dlgt) => true);
        }
        #endregion

    }
}
