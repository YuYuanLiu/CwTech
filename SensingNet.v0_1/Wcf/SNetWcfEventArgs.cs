using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.Wcf
{
    public class SNetWcfEventArgs : EventArgs
    {
        public SNnetWcfMessage Message;
        public SNnetWcfMessage ReplyMessage;
    }
}
