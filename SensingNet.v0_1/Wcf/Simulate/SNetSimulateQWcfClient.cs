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

namespace SensingNet.v0_1.Wcf.Simulate
{
    public class SNetSimulateQWcfClient : IDisposable
    {
        SNetWcfClient client;
        public volatile bool IsSendRequest = false;

        ~SNetSimulateQWcfClient() { this.Dispose(false); }

        public void RunAsyn()
        {
            this.client = new SNetWcfClient();
            this.client.evtReceiveData += (ss, ee) =>
            {
                CtkLog.InfoNs(this, ee.Message.DataObj + "");
            };
            this.client.Open(SNetSimulateQWcfListener.NetTcpUri);
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

            this.client.Channel.Send(new SNnetWcfMessage() { DataObj = "Client Send" });

        }


        public void Close()
        {
            if (this.client != null)
            {
                using (var obj = this.client)
                    obj.Close();

                this.client = null;
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
