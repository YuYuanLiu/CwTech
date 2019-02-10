using CToolkit.v0_1;
using CToolkit.v0_1.Logging;
using CToolkit.v0_1.Net;
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
    public class SNetSimulateDeviceVibration : IDisposable
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


            var len = 512;
            var sampleRate = 512.0;

            var sin1 = new DenseVector(Generate.Sinusoidal(len, sampleRate, 10.0, 1.0));
            var sin2 = new DenseVector(Generate.Sinusoidal(len, sampleRate, 60.0, 0.5));
            var wave = sin1 + sin2;
            var waveIndex = 0;


            DateTime? prevTime = DateTime.Now;
            this.listener = new CtkNonStopTcpListener("127.0.0.1", 5003);
            listener.NonStopConnectAsyn();

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
                sb.Append("cmd -respData -svid 0 -data");

                var now = DateTime.Now;
                var ts = now - prevTime.Value;
                prevTime = now;

                var limit = ts.Ticks * 1.0 / TimeSpan.TicksPerSecond * sampleRate;
                if (limit <= 0) limit = 1;
                if (ts.TotalMilliseconds > 500) limit = 1;

                for (var idx = 0; idx < limit; idx++)
                {
                    sb.AppendFormat(" {0}", wave[waveIndex++]);
                    if (waveIndex >= wave.Count) waveIndex = 0;
                }
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
