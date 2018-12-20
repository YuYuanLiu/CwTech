using CToolkit.v0_1.Secs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_0.Secs
{
  public  class EventArgsSecsRcvData:EventArgs
    {
        public QSecsHandler handler;
        public CtkHsmsMessage message;

    }
}
