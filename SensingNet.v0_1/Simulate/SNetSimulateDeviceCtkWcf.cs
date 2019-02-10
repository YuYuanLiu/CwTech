using CToolkit.v0_1;
using CToolkit.v0_1.Logging;
using CToolkit.v0_1.Net;
using CToolkit.v0_1.Wcf;
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
    public class SNetSimulateDeviceCtkWcf : IDisposable
    {
        CtkWcfDuplexTcpListener<CtkWcfDuplexTcpListener> listener;

        public const string ServerUri = @"net.tcp://localhost:9000/";



        ~SNetSimulateDeviceCtkWcf() { this.Dispose(false); }

        public void RunAsyn()
        {
            this.listener = CtkWcfDuplexTcpListener.CreateSingle();
            this.listener.evtDataReceive += (ss, ee) =>
            {
                var ea = ee as CtkWcfDuplexEventArgs;
                CmdWrite(ea.WcfMsg.DataObj + "");

            };
            this.listener.Uri = ServerUri;
            this.listener.ConnectIfNo();


        }




        public void CmdWrite(string msg, params object[] obj)
        {
            if (msg != null)
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
            this.listener.GetAllChannels().ForEach(row => row.CtkSend("Hello, I am server"));
        }


        public void Close()
        {
            using (var obj = this.listener)
                obj.Close();


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
