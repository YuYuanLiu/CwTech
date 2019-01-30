using CToolkit.v0_1.Timing;
using SensingNet.v0_1.Dsp.TimeSignal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.Dsp.Block
{
    public class SNetDspBlockTimeSignalSetSecondEventArg : SNetDspBlockTimeSignalSetEventArg
    {
        public SNetDspTimeSignalSetSecond TSignal;
        public SNetDspTimeSignalSetSecond NewTSignal= new SNetDspTimeSignalSetSecond();

    }
}
