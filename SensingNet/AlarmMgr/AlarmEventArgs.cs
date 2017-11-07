using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.AlarmMgr
{
    public class AlarmEventArgs : EventArgs
    {
        public AlarmHandler handler;
        public SignalEventArgs signalEventArgs;

    }
}
