using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.QWcf
{
    public class SNetQWcfClientCallback : ISNetQWcfClient
    {

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
