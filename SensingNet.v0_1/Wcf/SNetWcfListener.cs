using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace SensingNet.v0_1.Wcf
{


    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class SNetWcfListener : IDisposable
    {
        SNetWcfListenerService service = new SNetWcfListenerService();
        ServiceHost host;



        public List<ISNetWcfClient> AllChannels() { return this.service.AllChannels(); }

        public void Close()
        {
            if (this.service != null)
            {
                using (var obj = this.service)
                    obj.Close();
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
            var ep2 = this.host.AddServiceEndpoint(typeof(ISNetWcfListener), new NetTcpBinding(), address);
            //this.host2.Opened += (ss, ee) => { Console.WriteLine("The calculator service has begun to listen"); };
            this.host.Open();
        }






        #region Event

        public event EventHandler<SNetWcfEventArgs> evtReceiveData { add { this.service.evtReceiveData += value; } remove { this.service.evtReceiveData -= value; } }
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
