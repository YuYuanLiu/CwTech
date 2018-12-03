using CToolkit.Secs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.QSecs
{
    public class EventArgsSecsRcvData : EventArgs
    {
        public QSecsHandler handler;
        public HsmsMessage message;

    }
}
