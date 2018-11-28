using CToolkit;
using CToolkit.Net;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SensingNet.v0_1.Protocol
{

    /// <summary>
    /// 僅進行連線通訊, 不處理Protocol Format
    /// </summary>
    public class ProtoConnTcp : IProtoConnectBase, IDisposable
    {    //Socket m_connSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        CtkNonStopTcpClient client;
        CtkNonStopTcpListener listener;
        public TcpClient tcpClient;//一次只有一個可以被使用



        public bool IsConnected { get { return this.client != null ? this.client.isConnected : this.listener != null ? this.listener.IsConnected : false; } }
        public bool IsConnecting { get { return this.client != null ? this.client.IsConnecting : this.listener != null ? this.listener.IsConnecting : false; } }
        public bool IsTcpClientConnected { get { return this.tcpClient != null ? this.tcpClient.Connected : false; } }

        public bool isListener = true;

        public DateTime? timeOfBeginConnect;

        public IPEndPoint local;
        public IPEndPoint remote;

        //AutoResetEvent are_ConnectDone = new AutoResetEvent(false);
        ManualResetEvent mreHasMsg = new ManualResetEvent(false);

        public ProtoConnTcp(IPEndPoint l, IPEndPoint r, bool isListener)
        {
            this.local = l;
            this.remote = r;

            this.isListener = isListener;
        }


        ~ProtoConnTcp()
        {
            this.Dispose(false);
        }



        public void ConnectIfNo()
        {
            if (this.IsConnected || this.IsConnecting) return;

            var now = DateTime.Now;
            if (this.timeOfBeginConnect.HasValue && (now - this.timeOfBeginConnect.Value).TotalSeconds < 10) return;
            this.timeOfBeginConnect = DateTime.Now;

            if (this.isListener)
            {
                this.ReloadListener();
                this.listener.ConnectIfNo();
            }
            else
            {
                this.ReloadClient();
                this.client.ConnectIfNo();
            }


        }

        public void NonStopConnect()
        {
            if (this.IsConnected || this.IsConnecting) return;

            var now = DateTime.Now;
            if (this.timeOfBeginConnect.HasValue && (now - this.timeOfBeginConnect.Value).TotalSeconds < 10) return;
            this.timeOfBeginConnect = now;

            if (this.isListener)
            {
                this.ReloadListener();
                this.listener.NonStopConnect();
            }
            else
            {
                this.ReloadClient();
                this.client.NonStopConnect();
            }
        }

        public void Disconnect()
        {
            if (this.client != null) { this.client.Disconnect(); this.client.Dispose(); this.client = null; }
            if (this.listener != null) { this.listener.Disconnect(); this.listener.Dispose(); this.listener = null; }
            if (this.mreHasMsg != null) this.mreHasMsg.Dispose();

        }

        public void ReloadClient()
        {
            if (this.client != null) this.client.Disconnect();
            this.client = new CtkNonStopTcpClient();
            this.client.localEP = this.local;
            this.client.remoteEP = this.remote;
            this.client.evtFirstConnect += (sender, e) =>
            {
                this.tcpClient = e.tcpClient;
                this.OnFirstConnect(e);
            };
            this.client.evtFailConnect += (sender, e) => this.OnFailConnect(e);
            this.client.evtDisconnect += (sender, e) => this.OnDisconnect(e);
            this.client.evtDataReceive += (sender, e) => this.OnDataReceive(e);
        }
        public void ReloadListener()
        {
            if (this.listener != null) this.listener.Disconnect();
            this.listener = new CToolkit.Net.CtkNonStopTcpListener();
            this.listener.localEP = this.local;
            this.listener.evtFirstConnect += (sender, e) =>
            {
                this.tcpClient = e.tcpClient;
                this.OnFirstConnect(e);
            };
            this.listener.evtFailConnect += (sender, e) => this.OnFailConnect(e);
            this.listener.evtDisconnect += (sender, e) => this.OnDisconnect(e);
            this.listener.evtDataReceive += (sender, e) => this.OnDataReceive(e);
        }




        #region Event Handler

        #endregion 


        #region Event


        public event EventHandler<CtkNonStopTcpStateEventArgs> evtFirstConnect;
        void OnFirstConnect(CtkNonStopTcpStateEventArgs ea)
        {
            if (this.evtFirstConnect == null) return;
            this.evtFirstConnect(this, ea);
        }
        public event EventHandler<CtkNonStopTcpStateEventArgs> evtFailConnect;
        void OnFailConnect(CtkNonStopTcpStateEventArgs ea)
        {
            if (this.evtFailConnect == null) return;
            this.evtFailConnect(this, ea);
        }
        public event EventHandler<CtkNonStopTcpStateEventArgs> evtDisconnect;
        void OnDisconnect(CtkNonStopTcpStateEventArgs ea)
        {
            if (this.evtDisconnect == null) return;
            this.evtDisconnect(this, ea);
        }
        public event EventHandler<CtkNonStopTcpStateEventArgs> evtDataReceive;
        void OnDataReceive(CtkNonStopTcpStateEventArgs ea)
        {
            if (this.evtDataReceive == null) return;
            this.evtDataReceive(this, ea);
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
