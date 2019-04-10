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
        SNetWcfListenerService Service = new SNetWcfListenerService();
        ServiceHost host;



        public List<ISNetWcfClient> AllChannels() { return this.Service.AllChannels(); }

        public void Close()
        {
            if (this.Service != null)
            {
                using (var obj = this.Service)
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
            this.host = new ServiceHost(this.Service, new Uri(Path.Combine(uri)));
            var ep2 = this.host.AddServiceEndpoint(typeof(ISNetWcfListener), new NetTcpBinding(), address);
            //this.host2.Opened += (ss, ee) => { Console.WriteLine("The calculator service has begun to listen"); };
            this.host.Open();
        }






        #region Event

        public event EventHandler<SNetWcfEventArgs> evtReceiveData { add { this.Service.evtReceiveData += value; } remove { this.Service.evtReceiveData -= value; } }
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
