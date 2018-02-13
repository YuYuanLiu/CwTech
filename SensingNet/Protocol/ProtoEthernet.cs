using CToolkit.Logging;
using SensingNet.Protocol;
using SensingNet.Signal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SensingNet.Protocol
{
    public class ProtoEthernet : IDisposable
    {    //Socket m_connSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        TcpClient m_TcpClient;
        TcpListener m_TcpListener;
        TcpClient m_TcpListener_client;

        TcpClient m_ConnectTcpClient { get { return this.m_TcpClient == null ? this.m_TcpListener_client : this.m_TcpClient; } }
        public bool IsConnected { get { return this.m_ConnectTcpClient != null && this.m_ConnectTcpClient.Connected; } }


        public DateTime timeOfBeginConnect;

        public IPEndPoint local;
        public IPEndPoint remote;

        // Client  socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] Buffer = new byte[BufferSize];
        public int dataLength = 0;

        ProtoBase protoEthComm;


        Thread threadCommProc;// = new BackgroundWorker();
        public DeviceCfg dConfig;



        //AutoResetEvent are_ConnectDone = new AutoResetEvent(false);
        ManualResetEvent mreHasMsg = new ManualResetEvent(false);

        public ProtoEthernet(IPEndPoint l, IPEndPoint r, DeviceCfg dConfig)
        {
            this.local = l;
            this.remote = r;

            this.dConfig = dConfig;
            if (this.dConfig.TxMode == EnumProtocol.SensingNetCmd)
                this.protoEthComm = new ProtoSensingNetCmd();
            else if (this.dConfig.TxMode == EnumProtocol.Phd)
                this.protoEthComm = new ProtoPhd();
            else
                this.protoEthComm = new ProtoSecs();

            this.protoEthComm.dConfig = this.dConfig;
            this.protoEthComm.evtDataTrigger += delegate(object sender, EventArgs e) { this.OnDataRcv(e); };

        }


        ~ProtoEthernet()
        {
            this.Dispose(false);
        }



        public void ConnectIfNo()
        {

            if (!this.IsConnected)
            {
                if (this.dConfig.IsActivelyConnect)
                    this.DeviceIsActiveConnect();
                else
                    this.DeviceIsPassiveConnect();
            }

            //通訊活的話, 不用重啟
            if (this.threadCommProc == null || !this.threadCommProc.IsAlive)
                RestartCommunicationProcess();

        }
        public void Disconnect()
        {
            this.threadCommProc.Abort();
            if (this.m_TcpClient != null)
                this.m_TcpClient.Close();
            if (this.m_TcpListener_client != null)
                this.m_TcpListener_client.Close();
            if (this.m_TcpListener != null)
                this.m_TcpListener.Stop();


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


                    //若5秒內無法結束接收, 先往下走
                    /*
                    var timeoutRecord = DateTime.Now;
                    while (DateTime.Now - timeoutRecord < TimeSpan.FromSeconds(5))
                    {
                        //沒資料可以接收也往下走
                        if (!this.protoEthComm.IsReceiving()) break;
                    }
                    //等待資料接收處理序通知, 太久沒通知就往下走
                    */


                    this.mreHasMsg.WaitOne(this.dConfig.TimeoutResponse);
                    NetworkStream stream = this.m_ConnectTcpClient.GetStream();
                    this.protoEthComm.AnalysisData(stream);
                    if (this.protoEthComm.hasMessage())
                        this.mreHasMsg.Set();
                    else
                        this.mreHasMsg.Reset();


                    if (this.dConfig.IsActivelyTx)
                    {
                        this.protoEthComm.WriteMsg_TxDataAck(stream);
                    }
                    else
                    {
                        //等待下次要求資料的間隔
                        while ((timestampDiff = timestampEnd - timestampStart) < this.dConfig.TxInterval / 1000.0)
                            timestampEnd = CToolkit.DateTimeStamp.ToTimestamp();
                        timestampStart = timestampEnd;

                        this.protoEthComm.WriteMsg_TxDataReq(stream);
                    }



                }
                catch (Exception ex) { LoggerMapper.Singleton.WriteAsyn(ex); }
            }
        }


        void DeviceIsPassiveConnect()
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
        void DeviceIsActiveConnect()
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
            ProtoEthernet state = (ProtoEthernet)ar.AsyncState;
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
                    this.protoEthComm.FirstConnect(stream);
                    // Trigger the initial read.
                    stream.BeginRead(state.Buffer, 0, state.Buffer.Length, new AsyncCallback(EndReadCallback), state);
                }
                else
                {
                    LoggerMapper.Singleton.WriteAsyn(string.Format("Ready (last error: {0})", "Connect Failed!"));
                }
            }
            catch (SensingNetException ex)
            {
                LoggerMapper.Singleton.WriteAsyn(ex);
            }
            catch (NullReferenceException ex)
            {
                LoggerMapper.Singleton.Write(ex);
            }
        }

        void ServerEndConnectCallback(IAsyncResult ar)
        {
            ProtoEthernet state = (ProtoEthernet)ar.AsyncState;

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
                    this.protoEthComm.FirstConnect(stream);
                    // Trigger the initial read.
                    stream.BeginRead(state.Buffer, 0, state.Buffer.Length, new AsyncCallback(EndReadCallback), state);
                }
                else
                {
                    LoggerMapper.Singleton.WriteAsyn("Connect Failed!");
                }
            }
            catch (Exception ex) { LoggerMapper.Singleton.WriteAsyn(ex); }

        }

        void EndReadCallback(IAsyncResult ar)
        {
            try
            {


                ProtoEthernet state = (ProtoEthernet)ar.AsyncState;
                var client = state.m_ConnectTcpClient;
                if (client.Client == null || !client.Connected) return;
                NetworkStream stream = client.GetStream();

                // Call EndRead.
                int bytesRead = stream.EndRead(ar);

                protoEthComm.ReceiveBytes(state.Buffer, 0, bytesRead);
                if (this.protoEthComm.hasMessage())
                    this.mreHasMsg.Set();
                else
                    this.mreHasMsg.Reset();

                stream.BeginRead(state.Buffer, 0, state.Buffer.Length, new AsyncCallback(EndReadCallback), state);

            }
            catch (Exception ex) { CToolkit.Logging.LoggerMapper.Singleton.WriteAsyn(ex); }
        }



        #region Event

        public void CleanEvent()
        {
            foreach (Delegate d in this.evtCapture.GetInvocationList())
            {
                this.evtCapture -= (EventHandler<EventArgs>)d;
            }
        }



        public event EventHandler<EventArgs> evtCapture;
        public void OnDataRcv(EventArgs ea)
        {
            if (this.evtCapture == null)
                return;
            this.evtCapture(this, ea);
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
