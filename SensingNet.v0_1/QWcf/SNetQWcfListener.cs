using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace SensingNet.v0_1.QWcf
{


    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class SNetQWcfListener : IDisposable
    {
        SNetQWcfListenerService service = new SNetQWcfListenerService();
        Dictionary<string, SNetQWcfChannelInfo<ISNetQWcfClient>> ChannelMapper = new Dictionary<string, SNetQWcfChannelInfo<ISNetQWcfClient>>();
        ServiceHost host;



        public void Close()
        {
            foreach (var chinfo in this.ChannelMapper)
            {
                var ch = chinfo.Value.Channel;
                ch.Abort();
                ch.Close();
            }


            if (this.host != null)
            {
                using (var obj = this.host)
                {
                    obj.Abort();
                    obj.Close();
                }
            }
        }



        public void Open(string uri, string address = "")
        {
            this.host = new ServiceHost(this.service, new Uri(Path.Combine(uri)));
            var ep2 = this.host.AddServiceEndpoint(typeof(ISNetQWcfListener), new NetTcpBinding(), address);
            //this.host2.Opened += (ss, ee) => { Console.WriteLine("The calculator service has begun to listen"); };
            this.host.Open();
        }






        #region Event

        public event EventHandler<SNetQWcfEventArgs> evtReceiveData { add { this.service.evtReceiveData += value; } remove { this.service.evtReceiveData -= value; } }
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
