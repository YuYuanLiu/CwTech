using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace SensingNet.v0_1.Wcf
{
    public class SNetQWcfChannelInfo<T>
    {

        public T Callback;
        public string SessionId;
        public IContextChannel Channel;
    }
}
