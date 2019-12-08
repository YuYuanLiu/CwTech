using CToolkit;
using CToolkit.v1_0.Net;
using CToolkit.v1_0.Protocol;
using CToolkit.v1_0.Secs;
using CToolkit.v1_0.DigitalPort;
using System;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using CToolkit.v1_0;

namespace SensingNet.v0_1.Protocol
{

    /// <summary>
    /// 僅進行連線通訊, 不處理Protocol Format
    /// </summary>
    public class SNetProtoConnRs232 : ISNetProtoConnectBase, IDisposable
    {    //Socket m_connSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public CtkSerialPortCfg Config;
        public DateTime? timeOfBeginConnect;
        //AutoResetEvent are_ConnectDone = new AutoResetEvent(false);
        ManualResetEvent mreHasMsg = new ManualResetEvent(false);

        CtkNonStopSerialPort nonStopSerialPort;
        public SNetProtoConnRs232(CtkSerialPortCfg config)
        {
            this.Config = config;
        }


        ~SNetProtoConnRs232()
        {
            this.Dispose(false);
        }






        public void ReloadComPort()
        {

            if (this.nonStopSerialPort != null)
            {
                using (this.nonStopSerialPort)
                    this.nonStopSerialPort.Disconnect();
            }

            this.nonStopSerialPort = new CtkNonStopSerialPort(this.Config);
            this.nonStopSerialPort.evtFirstConnect += (sender, e) => { this.OnFirstConnect(e); };
            this.nonStopSerialPort.evtFailConnect += (sender, e) => this.OnFailConnect(e);
            this.nonStopSerialPort.evtDisconnect += (sender, e) => this.OnDisconnect(e);
            this.nonStopSerialPort.evtDataReceive += (sender, e) => this.OnDataReceive(e);
        }


        public void WriteBytes(byte[] buff, int offset, int length)
        {
            this.nonStopSerialPort.WriteMsg(new CtkProtocolBufferMessage()
            {
                Buffer = buff,
                Offset = offset,
                Length = length,
            });
        }



        #region IProtoConnectBase

        public event EventHandler<CtkProtocolEventArgs> evtDataReceive;
        public event EventHandler<CtkProtocolEventArgs> evtDisconnect;
        public event EventHandler<CtkProtocolEventArgs> evtErrorReceive;
        public event EventHandler<CtkProtocolEventArgs> evtFailConnect;
        public event EventHandler<CtkProtocolEventArgs> evtFirstConnect;

        public object ActiveWorkClient { get { return this.nonStopSerialPort.ActiveWorkClient; } set { this.nonStopSerialPort.ActiveWorkClient = value; } }
        public int IntervalTimeOfConnectCheck { get { return this.nonStopSerialPort.IntervalTimeOfConnectCheck; } set { this.nonStopSerialPort.IntervalTimeOfConnectCheck = value; } }
        public bool IsLocalReadyConnect { get { return this.nonStopSerialPort == null ? false : this.nonStopSerialPort.IsLocalReadyConnect; } }//Local連線成功=遠端連線成功
        public bool IsNonStopRunning { get { return this.nonStopSerialPort == null ? false : this.nonStopSerialPort.IsNonStopRunning; } }
        public bool IsOpenRequesting { get { return this.nonStopSerialPort == null ? false : this.nonStopSerialPort.IsOpenRequesting; } }
        public bool IsRemoteConnected { get { return this.nonStopSerialPort == null ? false : this.nonStopSerialPort.IsRemoteConnected; } }
        public void AbortNonStopConnect() { this.nonStopSerialPort.AbortNonStopConnect(); }

        //用途是避免重複要求連線
        public void ConnectIfNo()
        {
            if (this.IsNonStopRunning) return;//NonStopConnect 己在進行中的話, 不需再用ConnectIfNo
            if (this.IsRemoteConnected || this.IsOpenRequesting) return;

            var now = DateTime.Now;
            if (this.timeOfBeginConnect.HasValue && (now - this.timeOfBeginConnect.Value).TotalSeconds < 10) return;
            this.timeOfBeginConnect = DateTime.Now;

            this.ReloadComPort();
            this.nonStopSerialPort.ConnectIfNo();
        }
        public void Disconnect()
        {
            if (this.nonStopSerialPort != null) { this.nonStopSerialPort.Disconnect(); this.nonStopSerialPort.Dispose(); this.nonStopSerialPort = null; }
            if (this.mreHasMsg != null) this.mreHasMsg.Dispose();
        }

        public void NonStopConnectAsyn()
        {
            if (this.IsRemoteConnected || this.IsOpenRequesting) return;

            var now = DateTime.Now;
            if (this.timeOfBeginConnect.HasValue && (now - this.timeOfBeginConnect.Value).TotalSeconds < 10) return;
            this.timeOfBeginConnect = now;

            this.ReloadComPort();
            this.nonStopSerialPort.NonStopConnectAsyn();
        }
        public void WriteMsg(CtkProtocolTrxMessage msg)
        {
            if (msg.As<string>() != null)
            {
                var buff = Encoding.UTF8.GetBytes(msg.As<string>());
                this.WriteBytes(buff, 0, buff.Length);
            }
            else if (msg.As<CtkHsmsMessage>() != null)
            {
                var secsMsg = msg.As<CtkHsmsMessage>();
                var buffer = secsMsg.ToBytes();
                this.WriteBytes(buffer, 0, buffer.Length);
            }
            else
            {
                throw new ArgumentException("未定義該型別的寫入操作");
            }
        }
        void OnDataReceive(CtkProtocolEventArgs ea)
        {
            if (this.evtDataReceive == null) return;
            this.evtDataReceive(this, ea);
        }

        void OnDisconnect(CtkProtocolEventArgs ea)
        {
            if (this.evtDisconnect == null) return;
            this.evtDisconnect(this, ea);
        }

        void OnErrorReceive(CtkProtocolEventArgs ea)
        {
            if (this.evtErrorReceive == null) return;
            this.evtErrorReceive(this, ea);
        }

        void OnFailConnect(CtkProtocolEventArgs ea)
        {
            if (this.evtFailConnect == null) return;
            this.evtFailConnect(this, ea);
        }

        void OnFirstConnect(CtkProtocolEventArgs ea)
        {
            if (this.evtFirstConnect == null) return;
            this.evtFirstConnect(this, ea);
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
            }

            // Free any unmanaged objects here.
            //
            this.DisposeSelf();
            disposed = true;
        }



        void DisposeSelf()
        {
            this.Disconnect();
            CtkEventUtil.RemoveEventHandlersFromOwningByFilter(this, (dlgt) => true);
        }

        #endregion



    }
}
