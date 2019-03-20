using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.Wcf
{
    public class SNetWcfClientCallback : ISNetWcfClient
    {

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

        #region Event

        public event EventHandler<SNetWcfEventArgs> evtReceiveData;
        public void OnReceiveData(SNetWcfEventArgs ea)
        {
            if (this.evtReceiveData == null) return;
            this.evtReceiveData(this, ea);
        }



        #endregion
    }
}
