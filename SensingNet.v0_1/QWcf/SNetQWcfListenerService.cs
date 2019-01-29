using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace SensingNet.v0_1.QWcf
{
    public class SNetQWcfListenerService : ISNetQWcfListener
    {
        Dictionary<string, SNetQWcfChannelInfo<ISNetQWcfClient>> ChannelMapper = new Dictionary<string, SNetQWcfChannelInfo<ISNetQWcfClient>>();

        public void RemoveObsoleteChannel()
        {
            var query = (from row in this.ChannelMapper
                         where row.Value.Channel.State > CommunicationState.Opened
                         select row).ToList();

            foreach (var row in query)
                this.ChannelMapper.Remove(row.Key);
        }
        public List<ISNetQWcfClient> AllChannels()
        {
            this.RemoveObsoleteChannel();
            return (this.ChannelMapper.Select(row => row.Value.Callback)).ToList();
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

        #region ISNetQWcfListener
        public void Send(SNnetQWcfMessage msg)
        {
            this.OnReceiveData(new SNetQWcfEventArgs() { Message = msg });
        }

        public SNnetQWcfMessage SendRelay(SNnetQWcfMessage msg)
        {
            var ea = new SNetQWcfEventArgs() { Message = msg };
            this.OnReceiveData(ea);
            return ea.ReplyMessage;
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
    }
}
