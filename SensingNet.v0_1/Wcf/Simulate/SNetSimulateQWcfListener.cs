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

namespace SensingNet.v0_1.Wcf.Simulate
{
    public class SNetSimulateQWcfListener : IDisposable
    {
        SNetWcfListener listener;
        public const string NetTcpUri = @"net.tcp://localhost:9000/";

        ~SNetSimulateQWcfListener() { this.Dispose(false); }

        public void RunAsyn()
        {
            this.listener = new SNetWcfListener();
            this.listener.evtReceiveData += (ss, ee) =>
            {
                CmdWrite(ee.Message.Message);
            };
            this.listener.Open(NetTcpUri);
        }


        public void CmdWrite(string msg, params object[] obj)
        {
            if (!string.IsNullOrEmpty(msg))
            {
                Console.WriteLine();
                Console.WriteLine(msg, obj);
            }
            Console.Write(">");
        }

        public void CommandLine()
        {
            var cmd = "";
            do
            {
                CmdWrite(this.GetType().Name);
                cmd = Console.ReadLine();

                switch (cmd)
                {
                    case "send":
                        this.Send();
                        break;
                }


            } while (string.Compare(cmd, "exit", true) != 0);

            this.Close();

        }


        public void Send()
        {
            this.listener.AllChannels().ForEach(row => row.Send(new SNnetWcfMessage() { Message = "Listener Send" }));
        }


        public void Close()
        {
            if (this.listener != null)
            {
                using (var obj = this.listener)
                    obj.Close();
                this.listener = null;
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
            this.Close();
        }

        protected virtual void DisposeUnmanaged()
        {

        }
        #endregion


    }
}
