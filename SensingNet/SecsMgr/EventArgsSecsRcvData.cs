using CToolkit.Secs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.SecsMgr
{
  public  class EventArgsSecsRcvData:EventArgs
    {
        public QSecsHandler handler;
        public HsmsMessage message;

    }
}
