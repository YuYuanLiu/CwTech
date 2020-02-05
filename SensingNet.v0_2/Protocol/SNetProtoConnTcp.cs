using CodeExpress.v1_0.Secs;
using CToolkit;
using CToolkit.v1_0;
using CToolkit.v1_0.Net;
using CToolkit.v1_0.Protocol;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SensingNet.v0_2.Protocol
{

    /// <summary>
    /// 僅進行連線通訊, 不處理Protocol Format
    /// </summary>
    public class SNetProtoConnTcp : ISNetProtoConnectBase, IDisposable
    {    //Socket m_connSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        CtkNonStopTcpClient client;
        CtkNonStopTcpListener listener;
        TcpClient activeWorkTcpClient { get { return this.client.ActiveWorkClient as TcpClient; } }
        public NetworkStream ActiveWorkStream { get { return this.activeWorkTcpClient == null ? null : this.activeWorkTcpClient.GetStream(); } }
        ICtkProtocolNonStopConnect ctkProtoConnect { get { return this.client == null ? this.listener : this.client as ICtkProtocolNonStopConnect; } }
        public bool isListener = true;
        public DateTime? timeOfBeginConnect;
        public IPEndPoint LocalEndPoint;
        public IPEndPoint RemoteEndPoint;
        ManualResetEvent mreHasMsg = new ManualResetEvent(false);



        public SNetProtoConnTcp(IPEndPoint l, IPEndPoint r, bool isListener)
        {
            this.LocalEndPoint = l;
            this.RemoteEndPoint = r;

            this.isListener = isListener;
        }
        ~SNetProtoConnTcp()
        {
            this.Dispose(false);
        }




        public void ReloadClient()
        {
            if (this.client != null) this.client.Disconnect();
            this.client = new CtkNonStopTcpClient();
            this.client.localEP = this.LocalEndPoint;
            this.client.remoteEP = this.RemoteEndPoint;
            this.client.EhFirstConnect += (sender, e) =>
            {
                var ea = e as CtkNonStopTcpStateEventArgs;
                this.ActiveWorkClient = ea.workClient;
                this.OnFirstConnect(e);
            };
            this.client.EhFailConnect += (sender, e) => this.OnFailConnect(e);
            this.client.EhDisconnect += (sender, e) => this.OnDisconnect(e);
            this.client.EhDataReceive += (sender, e) => this.OnDataReceive(e);

            this.client.IntervalTimeOfConnectCheck = this.IntervalTimeOfConnectCheck;
        }
        public void ReloadListener()
        {
            if (this.listener != null) this.listener.Disconnect();
            this.listener = new CtkNonStopTcpListener();
            this.listener.localEP = this.LocalEndPoint;
            this.listener.EhFirstConnect += (sender, e) =>
            {
                var ea = e as CtkNonStopTcpStateEventArgs;
                this.ActiveWorkClient = ea.workClient;
                //this.listener.CleanExclude(this.activeWorkTcpClient);   
                this.OnFirstConnect(e);
            };
            this.listener.EhFailConnect += (sender, e) => this.OnFailConnect(e);
            this.listener.EhDisconnect += (sender, e) => this.OnDisconnect(e);
            this.listener.EhDataReceive += (sender, e) => this.OnDataReceive(e);

            this.listener.IntervalTimeOfConnectCheck = this.IntervalTimeOfConnectCheck;
        }

        public void WriteBytes(byte[] buff, int offset, int length)
        {
            var stream = this.ActiveWorkStream;
            stream.WriteTimeout = 10 * 1000;
            stream.Write(buff, offset, length);
            stream.Flush();
        }



        #region IProtoConnectBase

        public bool IsLocalReadyConnect { get { return this.ctkProtoConnect == null ? false : this.ctkProtoConnect.IsLocalReadyConnect; } }//Local連線成功=遠端連線成功
        public bool IsRemoteConnected { get { return this.ctkProtoConnect == null ? false : this.ctkProtoConnect.IsRemoteConnected; } }
        public bool IsOpenRequesting { get { return this.ctkProtoConnect == null ? false : this.ctkProtoConnect.IsOpenRequesting; } }//用途是避免重複要求連線
        public bool IsNonStopRunning { get { return this.ctkProtoConnect == null ? false : this.ctkProtoConnect.IsNonStopRunning; } }
        public int IntervalTimeOfConnectCheck { get; set; }

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
        public void NonStopConnectAsyn()
        {
            if (this.IsRemoteConnected || this.IsOpenRequesting) return;

            var now = DateTime.Now;
            //上次要求連線在10秒內也不會再連線
            if (this.timeOfBeginConnect.HasValue && (now - this.timeOfBeginConnect.Value).TotalSeconds < 10) return;
            this.timeOfBeginConnect = now;

            if (this.isListener)
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
        public void AbortNonStopConnect() { this.ctkProtoConnect.AbortNonStopConnect(); }
        public void Disconnect()
        {
            if (this.client != null) { this.client.Disconnect(); this.client.Dispose(); this.client = null; }
            if (this.listener != null) { this.listener.Disconnect(); this.listener.Dispose(); this.listener = null; }
            if (this.mreHasMsg != null) this.mreHasMsg.Dispose();

        }



        public object ActiveWorkClient { get { return this.ctkProtoConnect.ActiveWorkClient; } set { this.ctkProtoConnect.ActiveWorkClient = value; } }

        public void WriteMsg(CtkProtocolTrxMessage msg)
        {
            if (msg.As<string>() != null)
            {
                var buff = Encoding.UTF8.GetBytes(msg.As<string>());
                this.WriteBytes(buff, 0, buff.Length);
            }
            else if (msg.As<CxHsmsMessage>() != null)
            {
                var secsMsg = msg.As<CxHsmsMessage>();
                var buffer = secsMsg.ToBytes();
                this.WriteBytes(buffer, 0, buffer.Length);
            }
            else
            {
                throw new ArgumentException("未定義該型別的寫入操作");
            }
        }






        public event EventHandler<CtkProtocolEventArgs> EhFirstConnect;
        void OnFirstConnect(CtkProtocolEventArgs ea)
        {
            if (this.EhFirstConnect == null) return;
            this.EhFirstConnect(this, ea);
        }
        public event EventHandler<CtkProtocolEventArgs> EhFailConnect;
        void OnFailConnect(CtkProtocolEventArgs ea)
        {
            if (this.EhFailConnect == null) return;
            this.EhFailConnect(this, ea);
        }
        public event EventHandler<CtkProtocolEventArgs> EhDisconnect;
        void OnDisconnect(CtkProtocolEventArgs ea)
        {
            if (this.EhDisconnect == null) return;
            this.EhDisconnect(this, ea);
        }
        public event EventHandler<CtkProtocolEventArgs> EhDataReceive;
        void OnDataReceive(CtkProtocolEventArgs ea)
        {
            if (this.EhDataReceive == null) return;
            this.EhDataReceive(this, ea);
        }
        public event EventHandler<CtkProtocolEventArgs> EhErrorReceive;
        void OnErrorReceive(CtkProtocolEventArgs ea)
        {
            if (this.EhErrorReceive == null) return;
            this.EhErrorReceive(this, ea);
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
