using CToolkit;
using CToolkit.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SensingNet.v0_1.Protocol
{
    public class ProtoConnTcp : IDisposable
    {    //Socket m_connSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        CToolkit.Net.CtkNonStopTcpClient client;
        CToolkit.Net.CtkNonStopTcpListener listener;

        public bool IsConnected { get { return this.client != null ? this.client.isConnected : this.listener != null ? this.listener.isConnected : false; } }
        public bool isActivelyConnect = true;

        public DateTime timeOfBeginConnect;

        public IPEndPoint local;
        public IPEndPoint remote;

        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] Buffer = new byte[BufferSize];
        public int dataLength = 0;

        IProtoBase protoEthernet;
        Thread threadCommProc;// = new BackgroundWorker();


        //AutoResetEvent are_ConnectDone = new AutoResetEvent(false);
        ManualResetEvent mreHasMsg = new ManualResetEvent(false);

        public ProtoConnTcp(IPEndPoint l, IPEndPoint r, bool isActivelyConnect, IProtoBase proto)
        {
            this.local = l;
            this.remote = r;

            this.isActivelyConnect = isActivelyConnect;
            this.protoEthernet = proto;
            this.protoEthernet.evtDataTrigger += delegate (object sender, EventArgs e) { this.OnDataReceive(e); };
        }


        ~ProtoConnTcp()
        {
            this.Dispose(false);
        }



        public void ConnectIfNo()
        {
            if (!this.IsConnected)
            {
                if (this.isActivelyConnect)
                {
                    if (this.client != null) this.client.Disconnect();
                    this.client = new CToolkit.Net.CtkNonStopTcpClient();
                    this.client.ConnectIfNo();
                }
                else
                {
                    if (this.listener != null) this.listener.Disconnect();
                    this.listener = new CToolkit.Net.CtkNonStopTcpListener();
                    this.client.ConnectIfNo();
                }

            }

            //通訊活的話, 不用重啟
            if (this.threadCommProc == null || !this.threadCommProc.IsAlive)
                RestartCommunicationProcess();

        }
        public void NonStopConnect()
        {

        }


        public void Disconnect()
        {
            if (this.threadCommProc != null)
                this.threadCommProc.Abort();
            if (this.client != null) { this.client.Disconnect(); this.client.Dispose(); }
            if (this.listener != null) { this.listener.Disconnect(); this.listener.Dispose(); }
            this.CleanEvent();
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


            while (!disposed)
            {

                try
                {
                    if (this.m_ConnectTcpClient == null || !this.m_ConnectTcpClient.Connected) { System.Threading.Thread.Sleep(1000); continue; }


                    this.mreHasMsg.WaitOne(this.dConfig.TimeoutResponse);
                    NetworkStream stream = this.m_ConnectTcpClient.GetStream();
                    this.protoEthernet.AnalysisData(stream);
                    if (this.protoEthernet.hasMessage())
                        this.mreHasMsg.Set();//有訊息, 下一次也不用等
                    else
                        this.mreHasMsg.Reset();


                    if (this.dConfig.IsActivelyTx)
                    {
                        this.protoEthernet.WriteMsg_TxDataAck(stream);
                    }
                    else
                    {
                        //等待下次要求資料的間隔
                        while ((timestampDiff = timestampEnd - timestampStart) < this.dConfig.TxInterval / 1000.0)
                            timestampEnd = CToolkit.DateTimeStamp.ToTimestamp();
                        timestampStart = timestampEnd;

                        this.protoEthernet.WriteMsg_TxDataReq(stream);
                    }



                }
                catch (Exception ex) { LoggerAssembly.Write(ex); }
            }
        }


        void EndClientConnect_DeviceIsPassive()
        {
            if (this.m_TcpClient != null)
            {
                var ts = DateTime.Now - this.timeOfBeginConnect;
                if (ts.TotalSeconds < 5) return;
            }
            this.timeOfBeginConnect = DateTime.Now;

            if (this.m_ConnectTcpClient != null)
            {
                CToolkit.CtkUtil.TryCatch(() =>
                {
                    if (this.m_ConnectTcpClient.Connected && this.m_ConnectTcpClient.GetStream() != null)
                        this.m_ConnectTcpClient.GetStream().Close();
                    this.m_ConnectTcpClient.Close();
                });
            }

            if (this.local.Address == IPAddress.None)
                this.m_TcpClient = new TcpClient();
            else
                this.m_TcpClient = new TcpClient(this.local);

            this.m_TcpClient.NoDelay = true;
            this.m_TcpClient.BeginConnect(
                this.remote.Address,
                this.remote.Port,
                new AsyncCallback(ClientEndConnectCallback), this);
        }
        void EndServerConnect_DeviceIsActive()
        {
            if (this.m_TcpListener != null)
            {
                var ts = DateTime.Now - this.timeOfBeginConnect;
                if (ts.TotalSeconds < 5) return;
            }
            this.timeOfBeginConnect = DateTime.Now;

            if (this.m_ConnectTcpClient != null)
            {

                CToolkit.CtkUtil.TryCatch(() =>
                {
                    if (this.m_ConnectTcpClient.Connected && this.m_ConnectTcpClient.GetStream() != null)
                        this.m_ConnectTcpClient.GetStream().Close();
                    this.m_ConnectTcpClient.Close();
                });

            }


            if (this.m_TcpListener == null)
            {
                //this.m_TcpListener.Stop();
                //Listener 必填 Local
                this.m_TcpListener = new TcpListener(this.local);
                this.m_TcpListener.Start();
            }


            this.m_TcpListener.BeginAcceptTcpClient(new AsyncCallback(ServerEndConnectCallback), this);
            //this.m_TcpListener_client = this.m_TcpListener.AcceptTcpClient();
            //var stream = this.m_TcpListener_client.GetStream();


        }




        void ClientEndConnectCallback(IAsyncResult ar)
        {
            ProtoConnTcp state = (ProtoConnTcp)ar.AsyncState;
            TcpClient client = state.m_TcpClient;

            try
            {
                if (client.Client == null) return;
                if (!client.Connected) return;
                client.EndConnect(ar);
                NetworkStream stream = client.GetStream();

                if (client.Connected)
                {
                    //Console.WriteLine(string.Format("Ready Connect!"));
                    this.protoEthernet.FirstConnect(stream);
                    // Trigger the initial read.
                    stream.BeginRead(state.Buffer, 0, state.Buffer.Length, new AsyncCallback(EndReadCallback), state);
                }
                else
                {
                    LoggerAssembly.Write(string.Format("Ready (last error: {0})", "Connect Failed!"));
                }
            }
            catch (SensingNetException ex)
            {
                LoggerAssembly.Write(ex);
            }
            catch (NullReferenceException ex)
            {
                LoggerAssembly.Write(ex);
            }
        }

        void ServerEndConnectCallback(IAsyncResult ar)
        {
            ProtoConnTcp state = (ProtoConnTcp)ar.AsyncState;

            try
            {
                // End the operation and display the received data on 
                // the console.
                this.m_TcpListener_client = state.m_TcpListener.EndAcceptTcpClient(ar);
                NetworkStream stream = this.m_TcpListener_client.GetStream();

                // Process the connection here. (Add the client to a
                // server table, read data, etc.)
                if (this.m_TcpListener_client.Connected)
                {
                    //Console.WriteLine(string.Format("Ready Connect!"));
                    this.protoEthernet.FirstConnect(stream);
                    // Trigger the initial read.
                    stream.BeginRead(state.Buffer, 0, state.Buffer.Length, new AsyncCallback(EndReadCallback), state);
                }
                else
                {
                    LoggerAssembly.Write("Connect Failed!");
                }
            }
            catch (Exception ex) { LoggerAssembly.Write(ex); }

        }

        void EndReadCallback(IAsyncResult ar)
        {
            try
            {


                ProtoConnTcp state = (ProtoConnTcp)ar.AsyncState;
                var client = state.m_ConnectTcpClient;
                if (client.Client == null || !client.Connected) return;
                NetworkStream stream = client.GetStream();

                // Call EndRead.
                int bytesRead = stream.EndRead(ar);

                protoEthernet.ReceiveBytes(state.Buffer, 0, bytesRead);
                if (this.protoEthernet.hasMessage())
                    this.mreHasMsg.Set();//己完成一個訊息以上, 就不用等了
                else
                    this.mreHasMsg.Reset();

                stream.BeginRead(state.Buffer, 0, state.Buffer.Length, new AsyncCallback(EndReadCallback), state);

            }
            catch (Exception ex) { LoggerAssembly.Write(ex); }
        }



        #region Event

        public void CleanEvent()
        {

            foreach (Delegate d in this.evtDataReceive.GetInvocationList())
            {
                this.evtDataReceive -= (EventHandler<EventArgs>)d;
            }
        }



        public event EventHandler<EventArgs> evtDataReceive;
        public void OnDataReceive(EventArgs ea)
        {
            if (this.evtDataReceive == null)
                return;
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



        void DisposeManaged()
        {
            this.threadCommProc.Abort();
        }

        void DisposeUnmanaged()
        {

        }

        void DisposeSelf()
        {
            if (this.mreHasMsg != null)
                this.mreHasMsg.Dispose();
            if (this.m_TcpClient != null)
                this.m_TcpClient.Close();

        }

        #endregion



    }
}
