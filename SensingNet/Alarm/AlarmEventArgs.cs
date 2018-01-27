using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.Alarm
{
    public class AlarmEventArgs : EventArgs
    {
        public AlarmHandler handler;
        public SignalEventArgs signalEventArgs;

    }
}
