using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SensingNet.v0_1.QWcf
{


    public class SNetQWcfClient : ISNetQWcfClient, IDisposable
    {

        public ISNetQWcfListener Channel;
        public DuplexChannelFactory<ISNetQWcfListener> ChannelFactory;

        ~SNetQWcfClient() { this.Dispose(false); }


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
            var site = new InstanceContext(this);
            var endpointAddress = new EndpointAddress(Path.Combine(uri, address));
            var binding = new NetTcpBinding();
            this.ChannelFactory = new DuplexChannelFactory<ISNetQWcfListener>(site, binding, endpointAddress);
            this.Channel = this.ChannelFactory.CreateChannel();
        }
        #region ISNetQWcfListener

        public void Send(SNnetQWcfMessage msg)
        {
            this.OnReceiveData(new SNetQWcfEventArgs() { Message = msg });
        }


        #endregion


        #region Event

        public event EventHandler<SNetQWcfEventArgs> evtReceiveData;
        public void OnReceiveData(SNetQWcfEventArgs ea)
        {
            if (this.evtReceiveData == null) return;
            this.evtReceiveData(this, ea);
        }

        #endregion


        #region Dispose

        bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void DisposeManaged()
        {

        }

        public virtual void DisposeSelf()
        {
            this.Close();
        }

        public virtual void DisposeUnManaged()
        {
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any managed objects here.
                this.DisposeManaged();
            }

            // Free any unmanaged objects here.
            //
            this.DisposeUnManaged();
            this.DisposeSelf();
            disposed = true;
        }
        #endregion
    }




}
