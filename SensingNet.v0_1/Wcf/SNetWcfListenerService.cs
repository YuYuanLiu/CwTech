using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace SensingNet.v0_1.Wcf
{

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class SNetWcfListenerService : ISNetWcfListener,IDisposable
    {
        protected Dictionary<string, SNetQWcfChannelInfo<ISNetWcfClient>> ChannelMapper = new Dictionary<string, SNetQWcfChannelInfo<ISNetWcfClient>>();


        public void Close()
        {
            foreach (var chinfo in this.ChannelMapper)
            {
                var ch = chinfo.Value.Channel;
                ch.Abort();
                ch.Close();
            }

        }

        public void RemoveObsoleteChannel()
        {
            var query = (from row in this.ChannelMapper
                         where row.Value.Channel.State > CommunicationState.Opened
                         select row).ToList();

            foreach (var row in query)
                this.ChannelMapper.Remove(row.Key);
        }
        public List<ISNetWcfClient> AllChannels()
        {
            this.RemoveObsoleteChannel();
            return (this.ChannelMapper.Select(row => row.Value.Callback)).ToList();
        }
        public ISNetWcfClient Get(string key = null)
        {
            this.RemoveObsoleteChannel();
            var oc = OperationContext.Current;
            if (key == null)
                key = oc.SessionId;
            if (this.ChannelMapper.ContainsKey(key)) return this.ChannelMapper[key].Callback;

            var chinfo = new SNetQWcfChannelInfo<ISNetWcfClient>();
            chinfo.SessionId = key;
            chinfo.Channel = oc.Channel;
            chinfo.Callback = oc.GetCallbackChannel<ISNetWcfClient>();
            this.ChannelMapper[key] = chinfo;
            return chinfo.Callback;
        }

        #region ISNetQWcfListener
        public void Send(SNnetWcfMessage msg)
        {
            this.OnReceiveData(new SNetWcfEventArgs() { Message = msg });
        }

        public SNnetWcfMessage SendRelay(SNnetWcfMessage msg)
        {
            var ea = new SNetWcfEventArgs() { Message = msg };
            this.OnReceiveData(ea);
            return ea.ReplyMessage;
        }
        #endregion

        #region Event

        public event EventHandler<SNetWcfEventArgs> evtReceiveData;
        public void OnReceiveData(SNetWcfEventArgs ea)
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
