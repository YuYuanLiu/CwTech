using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SensingNet
{
    public class SimulateCmdProto : IDisposable
    {
        TcpListener m_TcpListener;
        TcpClient m_AcceptTcpClient;



        public IPEndPoint local;
        public IPAddress localIp { get { return this.local.Address; } set { this.local.Address = value; } }
        public Int32 localPort { get { return this.local.Port; } set { this.local.Port = value; } }
        public IPEndPoint remote;
        public IPAddress remoteIp { get { return this.remote.Address; } set { this.remote.Address = value; } }
        public Int32 remotePort { get { return this.remote.Port; } set { this.remote.Port = value; } }

        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        public int dataLength = 0;
        // Received data string.
        public StringBuilder rcvSb = new StringBuilder();
        List<UInt32> rcvBytes = new List<UInt32>();

        Thread bgWorkerTxReq;// = new BackgroundWorker();


        public bool IsConnected { get { return this.m_AcceptTcpClient != null && this.m_AcceptTcpClient.Connected; } }


        public SimulateCmdProto(String ip, Int32 port)
        {
            this.local = new IPEndPoint(IPAddress.Parse(ip), port);
        }
        public SimulateCmdProto(IPEndPoint l)
        {
            this.local = l;
        }

        ~SimulateCmdProto()
        {
            this.StopConnect();
            this.Dispose(false);
        }




        public void StartConnect()
        {
            if (this.m_TcpListener != null)
            {
                this.m_TcpListener.Stop();
            }
            this.m_TcpListener = new TcpListener(this.local);



            if (this.bgWorkerTxReq != null)
            {
                this.bgWorkerTxReq.Abort();
            }
            this.bgWorkerTxReq = new Thread(new ThreadStart(delegate ()
            {

                this.m_TcpListener.Start();


                while (!disposed)
                {

                    try
                    {
                        this.m_AcceptTcpClient = this.m_TcpListener.AcceptTcpClient();
                        var stream = this.m_AcceptTcpClient.GetStream();


                        int readLength;
                        while ((readLength = stream.Read(this.buffer, 0, this.buffer.Length)) != 0)
                        {
                            // Translate data bytes to a ASCII string.
                            var data = System.Text.Encoding.UTF8.GetString(this.buffer, 0, readLength);
                            this.rcvSb.Append(data);

                            var msg = this.rcvSb.ToString();

                            if (msg.Contains("cmd -reqData"))
                            {
                                this.WriteMsg(stream,
                                    string.Format(
                                        "cmd -respData -svid 2 -data {0}",
                                        25.0 + new Random(DateTime.Now.Millisecond).NextDouble() - 0.5
                                    ));
                                this.rcvSb.Clear();
                            }
                            else if (this.rcvSb.Length > 32)
                            {
                                this.rcvSb = new StringBuilder(msg.Substring(this.rcvSb.Length / 2));
                            }
                        }

                        this.m_AcceptTcpClient.Close();

                    }
                    catch (Exception ex) { CToolkit.Logging.LoggerMapper.Singleton.WriteAsyn(ex); }
                }
            }));
            this.bgWorkerTxReq.Start();

        }



        public void StopConnect()
        {
            if (this.m_TcpListener != null)
            {
                this.m_TcpListener.Stop();
            }
        }




        public void WriteMsg(NetworkStream stream, String msg)
        {
            if (!stream.CanWrite)
                return;
            var buffer = Encoding.UTF8.GetBytes(msg);

            stream.Write(buffer, 0, buffer.Length);
        }
        public void WriteMsg(NetworkStream stream, byte[] buffer)
        {
            if (!stream.CanWrite)
                return;
            //stream.WriteTimeout = 1000 * 10;
            stream.Write(buffer, 0, buffer.Length);
            stream.Flush();
        }



        #region Event

        public event EventHandler<EventArgs> evtCapture;
        public void OnDataRcv(EventArgs ea)
        {
            if (this.evtCapture == null)
                return;
            this.evtCapture(this, ea);
        }
        public void CleanDataReceiveEvent()
        {
            foreach (Delegate d in this.evtCapture.GetInvocationList())
            {
                this.evtCapture -= (EventHandler<EventArgs>)d;
            }
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
            disposed = true;
        }



        void DisposeManaged()
        {
            this.bgWorkerTxReq.Abort();
        }

        void DisposeUnmanaged()
        {

        }

        #endregion



    }
}
