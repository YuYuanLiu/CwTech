using CToolkit.v0_1.TimeOp;
using SensingNet.v0_1.Dsp.TimeSignal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.Dsp.Block
{
    public class SNetDspBlockTimeSignalSetSecondEventArg : EventArgs
    {
        public Object Sender;
        public CtkTimeSecond Time;
        public SNetDspTimeSignalSetSecond TimeSignal;

    }
}
