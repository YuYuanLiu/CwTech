using CToolkit.v1_0;
using CToolkit.v1_0.Logging;
using CToolkit.v1_0.Net;
using CToolkit.v1_0.Secs;
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
                CtkLog.InfoNs(this, ee.Message.DataObj + "");
            };
            this.listener.Open(NetTcpUri);
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
            this.listener.AllChannels().ForEach(row => row.Send(new SNnetWcfMessage() { DataObj = "Listener Send" }));
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
