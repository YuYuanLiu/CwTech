using CToolkit;
using CToolkit.Net;
using System;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SensingNet.v0_1.Protocol
{

    /// <summary>
    /// 僅進行連線通訊, 不處理Protocol Format
    /// </summary>
    public class ProtoConnRs232 : IProtoConnectBase, IDisposable
    {    //Socket m_connSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        CtkNonStopSerialPort nonStopSerialPort;
        public string ComPort;

        public bool IsConnected { get { return this.nonStopSerialPort.IsConnected; } }
        public bool IsConnecting { get { return this.nonStopSerialPort.IsConnecting; } }

        public DateTime? timeOfBeginConnect;

        //AutoResetEvent are_ConnectDone = new AutoResetEvent(false);
        ManualResetEvent mreHasMsg = new ManualResetEvent(false);

        public ProtoConnRs232(string compPort)
        {
            this.ComPort = compPort;
        }


        ~ProtoConnRs232()
        {
            this.Dispose(false);
        }



        public void ConnectIfNo()
        {
            if (this.IsConnected || this.IsConnecting) return;

            var now = DateTime.Now;
            if (this.timeOfBeginConnect.HasValue && (now - this.timeOfBeginConnect.Value).TotalSeconds < 10) return;
            this.timeOfBeginConnect = DateTime.Now;

            this.ReloadComPort();
            this.nonStopSerialPort.ConnectIfNo();
        }

        public void NonStopConnect()
        {
            if (this.IsConnected || this.IsConnecting) return;

            var now = DateTime.Now;
            if (this.timeOfBeginConnect.HasValue && (now - this.timeOfBeginConnect.Value).TotalSeconds < 10) return;
            this.timeOfBeginConnect = now;

            this.ReloadComPort();
            this.nonStopSerialPort.NonStopConnect();
        }




        public void Disconnect()
        {
            if (this.nonStopSerialPort != null) { this.nonStopSerialPort.Disconnect(); this.nonStopSerialPort.Dispose(); this.nonStopSerialPort = null; }
            if (this.mreHasMsg != null) this.mreHasMsg.Dispose();
        }


        public void ReloadComPort()
        {

            if (this.nonStopSerialPort != null)
            {
                using (this.nonStopSerialPort)
                    this.nonStopSerialPort.Disconnect();
            }

            this.nonStopSerialPort = new CtkNonStopSerialPort(this.ComPort);
            this.nonStopSerialPort.evtFirstConnect += (sender, e) => { this.OnFirstConnect(e); };
            this.nonStopSerialPort.evtFailConnect += (sender, e) => this.OnFailConnect(e);
            this.nonStopSerialPort.evtDisconnect += (sender, e) => this.OnDisconnect(e);
            this.nonStopSerialPort.evtDataReceive += (sender, e) => this.OnDataReceive(e);
        }





        #region Event Handler

        #endregion


        #region Event


        public event EventHandler<CtkNonStopSerialPortEventArgs> evtFirstConnect;
        void OnFirstConnect(CtkNonStopSerialPortEventArgs ea)
        {
            if (this.evtFirstConnect == null) return;
            this.evtFirstConnect(this, ea);
        }
        public event EventHandler<CtkNonStopSerialPortEventArgs> evtFailConnect;
        void OnFailConnect(CtkNonStopSerialPortEventArgs ea)
        {
            if (this.evtFailConnect == null) return;
            this.evtFailConnect(this, ea);
        }
        public event EventHandler<CtkNonStopSerialPortEventArgs> evtDisconnect;
        void OnDisconnect(CtkNonStopSerialPortEventArgs ea)
        {
            if (this.evtDisconnect == null) return;
            this.evtDisconnect(this, ea);
        }
        public event EventHandler<CtkNonStopSerialPortEventArgs> evtDataReceive;
        void OnDataReceive(CtkNonStopSerialPortEventArgs ea)
        {
            if (this.evtDataReceive == null) return;
            this.evtDataReceive(this, ea);
        }
        public event EventHandler<CtkNonStopSerialPortEventArgs> evtErrorReceive;
        void OnErrorReceive(CtkNonStopSerialPortEventArgs ea)
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
