using CToolkit.v1_0;
using CToolkit.v1_0.Logging;
using CToolkit.v1_0.Net;
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
    public class SNetSimulateDeviceRandom : IDisposable
    {
        public CtkNonStopTcpListener listener;

        public void RunAsyn()
        {

            CtkLog.RegisterAllLogger((ss, ea) =>
            {
                var now = DateTime.Now;
                var sb = new StringBuilder();
                sb.AppendFormat("[{0}] ", now.ToString("yyyyMMdd HH:mm:ss"));
                sb.AppendFormat("{0} ", ea.Message);
                sb.AppendFormat("{0}", ea.Exception.StackTrace);
                Write(sb.ToString());
            });



            DateTime? prevTime = DateTime.Now;
            this.listener = new CtkNonStopTcpListener("127.0.0.1", 5003);
            listener.NonStopConnectAsyn();
            var rnd = new Random((int)DateTime.Now.Ticks);

            listener.evtFirstConnect += (ss, ee) =>
            {
                var myea = ee as CtkNonStopTcpStateEventArgs;
                var sb = new StringBuilder();
                sb.Append("evtFirstConnect:\n");
                sb.Append(this.CmdState());
                this.Write(sb.ToString());
            };
            listener.evtDataReceive += (ss, ee) =>
            {
                var myea = ee as CtkNonStopTcpStateEventArgs;
                var ctkBuffer = myea.TrxMessageBuffer;
                var msg = Encoding.UTF8.GetString(ctkBuffer.Buffer, ctkBuffer.Offset, ctkBuffer.Length);
                if (!msg.Contains("\n")) return;
                var sb = new StringBuilder();
                sb.Append("cmd -respData -svid 0 -data ");
                sb.Append(rnd.NextDouble());



                sb.AppendLine();

                myea.WriteMsg(sb.ToString());

            };



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
                    case "state":
                        Write(this.CmdState());
                        break;
                }


            } while (string.Compare(cmd, "exit", true) != 0);

            this.Stop();
        }

        string CmdState()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("Connected Count={0}\n", this.listener.ConnectCount());
            sb.AppendFormat("Client Count={0}\n", this.listener.TcpClientList.Count);
            return sb.ToString();
        }


        void Write(string msg, params object[] arg)
        {
            Console.WriteLine();
            Console.WriteLine(msg, arg);
            Console.Write(">");
        }


        public void Stop()
        {
            this.listener.AbortNonStopConnect();
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
