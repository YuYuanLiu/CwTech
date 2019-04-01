using CToolkit;
using CToolkit.v1_0;
using CToolkit.v1_0.Net;
using CToolkit.v1_0.Protocol;
using CToolkit.v1_0.Secs;
using CToolkit.v1_0.Wcf;
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
    public class SNetProtoConnTcpWcf : ISNetProtoConnectBase, IDisposable
    {    //Socket m_connSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public bool IsListener = true;
        public DateTime? timeOfBeginConnect;
        public string Uri;
        protected int m_IntervalTimeOfConnectCheck = 5000;
        CtkWcfDuplexTcpClient<ICtkWcfDuplexOpService, CtkWcfDuplexTcpClient> client;
        CtkWcfDuplexTcpListener<ICtkWcfDuplexOpService> listener;

        public SNetProtoConnTcpWcf(string uri, bool isListener)
        {
            this.Uri = uri;
            this.IsListener = isListener;
        }

        ~SNetProtoConnTcpWcf()
        {
            this.Dispose(false);
        }

        ICtkProtocolNonStopConnect ctkProtoConnect { get { return this.client == null ? this.listener : this.client as ICtkProtocolNonStopConnect; } }

        void ReloadClient()
        {
            if (this.client != null) this.client.Disconnect();
            this.client = CtkWcfDuplexTcpClient.NewDefault();

            this.client.evtFirstConnect += (ss, ee) => this.OnFirstConnect(ee);
            this.client.evtFailConnect += (ss, ee) => this.OnFailConnect(ee);
            this.client.evtDisconnect += (ss, ee) => this.OnDisconnect(ee);
            this.client.evtDataReceive += (ss, ee) => this.OnDataReceive(ee);
            this.client.evtErrorReceive += (ss, ee) => this.OnErrorReceive(ee);

        }
        void ReloadListener()
        {
            if (this.listener != null) this.listener.Disconnect();
            this.listener = CtkWcfDuplexTcpListener.NewDefault();
            this.listener.Uri = this.Uri;

            this.listener.evtFirstConnect += (ss, ee) => this.OnFirstConnect(ee);
            this.listener.evtFailConnect += (ss, ee) => this.OnFailConnect(ee);
            this.listener.evtDisconnect += (ss, ee) => this.OnDisconnect(ee);
            this.listener.evtDataReceive += (ss, ee) => this.OnDataReceive(ee);
            this.listener.evtErrorReceive += (ss, ee) => this.OnErrorReceive(ee);
        }



        #region ISNetProtoConnectBase

        public event EventHandler<CtkProtocolEventArgs> evtDataReceive;
        public event EventHandler<CtkProtocolEventArgs> evtDisconnect;
        public event EventHandler<CtkProtocolEventArgs> evtErrorReceive;
        public event EventHandler<CtkProtocolEventArgs> evtFailConnect;
        public event EventHandler<CtkProtocolEventArgs> evtFirstConnect;

        public object ActiveWorkClient { get { return this.ctkProtoConnect.ActiveWorkClient; } set { this.ctkProtoConnect.ActiveWorkClient = (ICtkProtocolNonStopConnect)value; } }
        public int IntervalTimeOfConnectCheck { get { return this.m_IntervalTimeOfConnectCheck; } set { this.m_IntervalTimeOfConnectCheck = value; } }
        public bool IsLocalReadyConnect { get { return this.ctkProtoConnect != null && this.ctkProtoConnect.IsLocalReadyConnect; } }//Local連線成功=遠端連線成功
        public bool IsNonStopRunning { get { return this.ctkProtoConnect != null && this.ctkProtoConnect.IsNonStopRunning; } }
        public bool IsOpenRequesting { get { return this.ctkProtoConnect != null && this.ctkProtoConnect.IsOpenRequesting; } }
        public bool IsRemoteConnected { get { return this.ctkProtoConnect != null && this.ctkProtoConnect.IsRemoteConnected; } }
        public void AbortNonStopConnect() { this.ctkProtoConnect.AbortNonStopConnect(); }

        //用途是避免重複要求連線
        public void ConnectIfNo()
        {
            if (this.IsNonStopRunning) return;//NonStopConnect 己在進行中的話, 不需再用ConnectIfNo
            if (this.IsRemoteConnected || this.IsOpenRequesting) return;

            var now = DateTime.Now;
            if (this.timeOfBeginConnect.HasValue && (now - this.timeOfBeginConnect.Value).TotalSeconds < 10) return;
            this.timeOfBeginConnect = DateTime.Now;

            if (this.IsListener)
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
        public void Disconnect()
        {
            if (this.client != null) { this.client.Disconnect(); this.client.Dispose(); this.client = null; }
            if (this.listener != null) { this.listener.Disconnect(); this.listener.Dispose(); this.listener = null; }

            CtkEventUtil.RemoveEventHandlersFromOwningByFilter(this, (dlgt) => true);
        }

        public void NonStopConnectAsyn()
        {
            if (this.IsRemoteConnected || this.IsOpenRequesting) return;

            var now = DateTime.Now;
            //上次要求連線在10秒內也不會再連線
            if (this.timeOfBeginConnect.HasValue && (now - this.timeOfBeginConnect.Value).TotalSeconds < 10) return;
            this.timeOfBeginConnect = now;

            if (this.IsListener)
            {
                this.ReloadListener();
                this.listener.NonStopConnectAsyn();
            }
            else
            {
                this.ReloadClient();
                this.client.NonStopConnectAsyn();
            }
        }
        //一次只有一個可以被使用
        public void WriteMsg(CtkProtocolTrxMessage msg)
        {
            if (msg.As<CtkWcfMessage>() != null)
            {
                this.ctkProtoConnect.WriteMsg(msg);
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
                this.DisposeManaged();
            }

            // Free any unmanaged objects here.
            //
            this.DisposeUnmanaged();
            this.DisposeSelf();
            disposed = true;
        }



        void DisposeManaged() { }
        void DisposeSelf()
        {
            this.Disconnect();
            CtkEventUtil.RemoveEventHandlersFromOwningByFilter(this, (dlgt) => true);
        }

        void DisposeUnmanaged() { }
        #endregion



    }
}
