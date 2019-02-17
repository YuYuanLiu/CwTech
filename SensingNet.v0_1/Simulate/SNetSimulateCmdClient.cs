using CToolkit.v0_1;
using CToolkit.v0_1.Logging;
using CToolkit.v0_1.Net;
using CToolkit.v0_1.Secs;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SensingNet.v0_1.Simulate
{
    public class SNetSimulateCmdClient : IDisposable
    {
        CtkNonStopTcpClient client;
        public volatile bool IsSendRequest = false;

        ~SNetSimulateCmdClient() { this.Dispose(false); }

        public void RunAsyn()
        {

            client = new CtkNonStopTcpClient("127.0.0.1", 5003);
            client.evtFirstConnect += (ss, ee) => { Write("evtFirstConnect"); };
            client.evtFailConnect += (ss, ee) =>
            {
                var sb = new StringBuilder();
                sb.Append("evtFailConnect: ");
                sb.Append(ee.Exception.StackTrace);
                Write(sb.ToString());
            };
            client.evtErrorReceive += (ss, ee) => { Write("evtErrorReceive"); };
            client.evtDataReceive += (ss, ee) =>
            {
                var ea = ee as CtkNonStopTcpStateEventArgs;
                var ctkBuffer = ea.TrxMessageBuffer;
                var msg = Encoding.UTF8.GetString(ctkBuffer.Buffer, ctkBuffer.Offset, ctkBuffer.Length);
                Write(msg);
            };

            client.NonStopConnectAsyn();
        }


        public void Write(string msg, params object[] obj)
        {
            Console.WriteLine();
            Console.WriteLine(msg, obj);
            Console.Write(">");
        }

        public void CommandLine()
        {
            var cmd = "";
            do
            {
                Write(this.GetType().Name);
                cmd = Console.ReadLine();

                switch (cmd)
                {
                    case "send":
                        this.Send();
                        break;
                    case "state":
                        Console.WriteLine("State={0}", this.client.IsRemoteConnected);
                        break;
                }


            } while (string.Compare(cmd, "exit", true) != 0);

            this.Stop();

        }


        public void Send()
        {

            this.client.WriteMsg("cmd\n");

        }


        public void Stop()
        {
            if (this.client != null)
            {
                using (this.client)
                {
                    this.client.AbortNonStopConnect();
                    this.client.Disconnect();
                }
            }
        }


        #region IDisposable
        // Flag: Has Dispose already been called?
        protected bool disposed = false;

        // Public implementation of Dispose pattern callable by consumers.
        public virtual void Dispose()
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



        protected virtual void DisposeManaged()
        {
        }

        protected virtual void DisposeSelf()
        {
            this.Stop();
        }

        protected virtual void DisposeUnmanaged()
        {

        }
        #endregion


    }
}
