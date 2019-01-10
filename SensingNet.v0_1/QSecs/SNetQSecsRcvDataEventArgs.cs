using CToolkit.v0_1.Secs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.QSecs
{
    public class SNetQSecsRcvDataEventArgs : EventArgs
    {
        public SNetQSecsHandler handler;
        public CtkHsmsMessage message;

    }
}
