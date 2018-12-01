using CToolkit;
using CToolkit.Net;
using CToolkit.Protocol;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
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
        TcpClient activeWorkTcpClient { get => this.client.ActiveWorkClient as TcpClient; }
        public NetworkStream ActiveWorkStream { get => this.activeWorkTcpClient == null ? null : this.activeWorkTcpClient.GetStream(); }
        ICtkProtocolNonStopConnect ctkProtoConnect { get => this.client == null ? this.listener : this.client as ICtkProtocolNonStopConnect; }
        public bool isListener = true;
        public DateTime? timeOfBeginConnect;
        public IPEndPoint local;
        public IPEndPoint remote;
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




        public void ReloadClient()
        {
            if (this.client != null) this.client.Disconnect();
            this.client = new CtkNonStopTcpClient();
            this.client.localEP = this.local;
            this.client.remoteEP = this.remote;
            this.client.evtFirstConnect += (sender, e) =>
            {
                var ea = e as CtkNonStopTcpStateEventArgs;
                this.ActiveWorkClient = ea.workClient;
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
                var ea = e as CtkNonStopTcpStateEventArgs;
                this.ActiveWorkClient = ea.workClient;
                this.OnFirstConnect(e);
            };
            this.listener.evtFailConnect += (sender, e) => this.OnFailConnect(e);
            this.listener.evtDisconnect += (sender, e) => this.OnDisconnect(e);
            this.listener.evtDataReceive += (sender, e) => this.OnDataReceive(e);
        }




        #region IProtoConnectBase

        public bool IsLocalReadyConnect { get => this.ctkProtoConnect.IsLocalReadyConnect; }//Local連線成功=遠端連線成功
        public bool IsRemoteConnected { get => this.ctkProtoConnect.IsRemoteConnected; }
        public bool IsOpenRequesting { get => this.ctkProtoConnect.IsOpenRequesting; }//用途是避免重複要求連線
        public bool IsNonStopRunning { get => this.ctkProtoConnect.IsNonStopRunning; }


        public void ConnectIfNo()
        {
            if (this.IsNonStopRunning) return;//NonStopConnect 己在進行中的話, 不需再用ConnectIfNo
            if (this.IsRemoteConnected || this.IsOpenRequesting) return;

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
            if (this.IsRemoteConnected || this.IsOpenRequesting) return;

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
        public void AbortNonStopConnect() { this.ctkProtoConnect.AbortNonStopConnect(); }
        public void Disconnect()
        {
            if (this.client != null) { this.client.Disconnect(); this.client.Dispose(); this.client = null; }
            if (this.listener != null) { this.listener.Disconnect(); this.listener.Dispose(); this.listener = null; }
            if (this.mreHasMsg != null) this.mreHasMsg.Dispose();

        }



        public object ActiveWorkClient { get => this.client == null ? this.listener.ActiveWorkClient : this.client.ActiveWorkClient; set => this.client.ActiveWorkClient = value; }//一次只有一個可以被使用
        public void WriteMsg(byte[] buff, int offset, int length) { this.ActiveWorkStream.Write(buff, offset, length); }
        public void WriteMsg(byte[] buff, int length) { this.WriteMsg(buff, 0, length); }
        public void WriteMsg(byte[] buff) { this.WriteMsg(buff, 0, buff.Length); }
        public void WriteMsg(String msg) { this.WriteMsg(Encoding.UTF8.GetBytes(msg)); }






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
