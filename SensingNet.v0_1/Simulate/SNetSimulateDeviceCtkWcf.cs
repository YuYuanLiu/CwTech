using CToolkit.v1_0;
using CToolkit.v1_0.Logging;
using CToolkit.v1_0.Net;
using CToolkit.v1_0.Wcf;
using CToolkit.v1_0.Wcf.DuplexTcp;
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
        CtkWcfDuplexTcpNonStopListener<ICtkWcfDuplexTcpService> listener;

        public const string ServerUri = @"net.tcp://localhost:9000/";



        ~SNetSimulateDeviceCtkWcf() { this.Dispose(false); }

        public void RunAsyn()
        {
            this.listener = new CtkWcfDuplexTcpNonStopListener<ICtkWcfDuplexTcpService>(new CtkWcfDuplexTcpListenerInst());
            this.listener.evtDataReceive += (ss, ee) =>
            {
                var ea = ee as CtkWcfDuplexEventArgs;
                CtkLog.InfoNs(this, ea.WcfMsg.TypeName + "");

            };
            this.listener.Uri = ServerUri;
            this.listener.ConnectIfNo();


        }



        public void Command(string cmd)
        {
            switch (cmd)
            {
                case "send":
                    this.Send();
                    break;
            }


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
            }

            // Free any unmanaged objects here.
            //

            this.DisposeSelf();

            disposed = true;
        }





        protected virtual void DisposeSelf()
        {
            this.Close();
        }



        #endregion


    }
}
