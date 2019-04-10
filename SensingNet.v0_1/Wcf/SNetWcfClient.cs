using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SensingNet.v0_1.Wcf
{


    public class SNetWcfClient : IDisposable
    {

        public ISNetWcfListener Channel;
        public DuplexChannelFactory<ISNetWcfListener> ChannelFactory;
        public SNetWcfClientCallback callback = new SNetWcfClientCallback();

        ~SNetWcfClient() { this.Dispose(false); }


        public SNetWcfClient()
        {

        }

        public void Close()
        {
            if (this.ChannelFactory != null)
            {
                using (var obj = this.ChannelFactory)
                {
                    obj.Abort();
                    obj.Close();
                }
            }
        }

        public void Open(string uri, string address = "")
        {
            var site = new InstanceContext(this.callback);
            var endpointAddress = new EndpointAddress(Path.Combine(uri, address));
            var binding = new NetTcpBinding();
            this.ChannelFactory = new DuplexChannelFactory<ISNetWcfListener>(site, binding, endpointAddress);
            this.Channel = this.ChannelFactory.CreateChannel();
        }




        #region Event

        public event EventHandler<SNetWcfEventArgs> evtReceiveData { add { this.callback.evtReceiveData += value; } remove { this.callback.evtReceiveData -= value; } }

        #endregion


        #region Dispose

        bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }




        public virtual void DisposeSelf()
        {
            this.Close();
        }




        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any managed objects here.
            }

            // Free any unmanaged objects here.
            //
            this.DisposeSelf();
            disposed = true;
        }
        #endregion
    }




}
