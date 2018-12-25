using CToolkit.v0_1.TimeOp;
using SensingNet.v0_1.Dsp.TimeSignal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.Dsp.Block
{
    public class SNetDspBlockTimeSignalSetEventArg : SNetDspBlockTimeSignalEventArg
    {
        public CtkTimeSecond Time;
        public CtkTimeSecond? BeforeLastTime;
    }
}
