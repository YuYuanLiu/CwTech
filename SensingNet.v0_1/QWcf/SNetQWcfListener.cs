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
    public class SNetQWcfListener : ISNetQWcfListener, IDisposable
    {
        Dictionary<string, SNetQWcfChannelInfo<ISNetQWcfClient>> ChannelMapper = new Dictionary<string, SNetQWcfChannelInfo<ISNetQWcfClient>>();
        ServiceHost host;
        public List<ISNetQWcfClient> AllChannels()
        {
            this.RemoveObsoleteChannel();
            return (this.ChannelMapper.Select(row => row.Value.Callback)).ToList();
        }

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

        public ISNetQWcfClient Get(string key = null)
        {
            this.RemoveObsoleteChannel();
            var oc = OperationContext.Current;
            if (key == null)
                key = oc.SessionId;
            if (this.ChannelMapper.ContainsKey(key)) return this.ChannelMapper[key].Callback;

            var chinfo = new SNetQWcfChannelInfo<ISNetQWcfClient>();
            chinfo.SessionId = key;
            chinfo.Channel = oc.Channel;
            chinfo.Callback = oc.GetCallbackChannel<ISNetQWcfClient>();
            this.ChannelMapper[key] = chinfo;
            return chinfo.Callback;
        }

        public void Open(string uri, string address = "")
        {
            this.host = new ServiceHost(this, new Uri(Path.Combine(uri)));
            var ep2 = this.host.AddServiceEndpoint(typeof(ISNetQWcfListener), new NetTcpBinding(), address);
            //this.host2.Opened += (ss, ee) => { Console.WriteLine("The calculator service has begun to listen"); };
            this.host.Open();
        }

        public void RemoveObsoleteChannel()
        {
            var query = (from row in this.ChannelMapper
                         where row.Value.Channel.State > CommunicationState.Opened
                         select row).ToList();

            foreach (var row in query)
                this.ChannelMapper.Remove(row.Key);
        }
        #region ISNetQWcfListener

        public void Send(SNnetQWcfMessage msg)
        {
            var callback = this.Get();
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
