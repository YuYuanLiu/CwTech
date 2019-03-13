using CToolkit.v0_1.Timing;
using SensingNet.v0_1.Dsp.TimeSignal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.Dsp
{
    public class SNetDspSignalSetSecF8EventArg : SNetDspSignalEventArg
    {
        public CtkTimeSecond? Time;//當次時間
        public CtkTimeSecond? PrevTime;//前一次時間
        public SNetDspTSignalSetSecF8 TSignal;//完整訊號
        public SNetDspTSignalSetSecF8 NewTSignal = new SNetDspTSignalSetSecF8();//新增訊號

    }
}
