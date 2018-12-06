using CToolkit;
using CToolkit.Net;
using CToolkit.Protocol;
using CToolkit.Secs;
using CToolkit.v0_1.DigitalPort;
using System;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SensingNet.v0_1.Protocol
{

    /// <summary>
    /// 僅進行連線通訊, 不處理Protocol Format
    /// </summary>
    public class SNetProtoConnRs232 : ISNetProtoConnectBase, IDisposable
    {    //Socket m_connSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        CtkNonStopSerialPort nonStopSerialPort;
        public CtkSerialPortCfg Config;




        public DateTime? timeOfBeginConnect;

        //AutoResetEvent are_ConnectDone = new AutoResetEvent(false);
        ManualResetEvent mreHasMsg = new ManualResetEvent(false);

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





        #region IProtoConnectBase

        public bool IsLocalReadyConnect { get { return this.nonStopSerialPort == null ? false : this.nonStopSerialPort.IsLocalReadyConnect; } }//Local連線成功=遠端連線成功
        public bool IsRemoteConnected { get { return this.nonStopSerialPort == null ? false : this.nonStopSerialPort.IsRemoteConnected; } }
        public bool IsOpenRequesting { get { return this.nonStopSerialPort == null ? false : this.nonStopSerialPort.IsOpenRequesting; } }//用途是避免重複要求連線
        public bool IsNonStopRunning { get { return this.nonStopSerialPort == null ? false : this.nonStopSerialPort.IsNonStopRunning; } }



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
        public void NonStopConnectAsyn()
        {
            if (this.IsRemoteConnected || this.IsOpenRequesting) return;

            var now = DateTime.Now;
            if (this.timeOfBeginConnect.HasValue && (now - this.timeOfBeginConnect.Value).TotalSeconds < 10) return;
            this.timeOfBeginConnect = now;

            this.ReloadComPort();
            this.nonStopSerialPort.NonStopConnectAsyn();
        }
        public void AbortNonStopConnect() { this.nonStopSerialPort.AbortNonStopConnect(); }
        public void Disconnect()
        {
            if (this.nonStopSerialPort != null) { this.nonStopSerialPort.Disconnect(); this.nonStopSerialPort.Dispose(); this.nonStopSerialPort = null; }
            if (this.mreHasMsg != null) this.mreHasMsg.Dispose();
        }

        public object ActiveWorkClient { get { return this.nonStopSerialPort.ActiveWorkClient; } set { this.nonStopSerialPort.ActiveWorkClient = value; } }
        public void WriteBytes(byte[] buff, int offset, int length) { this.nonStopSerialPort.WriteBytes(buff, offset, length); }
        public void WriteBytes(byte[] buff, int length) { this.WriteBytes(buff, 0, length); }
        public void WriteBytes(byte[] buff) { this.WriteBytes(buff, 0, buff.Length); }
        public void WriteMsg(object msg)
        {
            if (msg.GetType() == typeof(string))
            {
                var buff = Encoding.UTF8.GetBytes(msg as string);
                this.WriteBytes(buff, 0, buff.Length);
            }
            else if (msg.GetType() == typeof(HsmsMessage))
            {
                var secsMsg = msg as HsmsMessage;
                this.WriteBytes(secsMsg.ToBytes());
            }
            else
            {
                throw new ArgumentException("未定義該型別的寫入操作");
            }
        }



        public event EventHandler<CtkProtocolBufferEventArgs> evtFirstConnect;
        void OnFirstConnect(CtkProtocolBufferEventArgs ea)
        {
            if (this.evtFirstConnect == null) return;
            this.evtFirstConnect(this, ea);
        }
        public event EventHandler<CtkProtocolBufferEventArgs> evtFailConnect;
        void OnFailConnect(CtkProtocolBufferEventArgs ea)
        {
            if (this.evtFailConnect == null) return;
            this.evtFailConnect(this, ea);
        }
        public event EventHandler<CtkProtocolBufferEventArgs> evtDisconnect;
        void OnDisconnect(CtkProtocolBufferEventArgs ea)
        {
            if (this.evtDisconnect == null) return;
            this.evtDisconnect(this, ea);
        }
        public event EventHandler<CtkProtocolBufferEventArgs> evtDataReceive;
        void OnDataReceive(CtkProtocolBufferEventArgs ea)
        {
            if (this.evtDataReceive == null) return;
            this.evtDataReceive(this, ea);
        }
        public event EventHandler<CtkProtocolBufferEventArgs> evtErrorReceive;
        void OnErrorReceive(CtkProtocolBufferEventArgs ea)
        {
            if (this.evtErrorReceive == null) return;
            this.evtErrorReceive(this, ea);
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



        void DisposeManaged() { }
        void DisposeUnmanaged() { }

        void DisposeSelf()
        {
            this.Disconnect();
            EventUtil.RemoveEventHandlersFrom(delegate (Delegate dlgt) { return true; }, this);
        }


        #endregion



    }
}
