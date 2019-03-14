using CToolkit.v0_1.Timing;
using SensingNet.v0_1.TriggerDiagram.TimeSignal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.TriggerDiagram
{
    public class SNetTdSignalSetSecF8EventArg : SNetTdSignalEventArg
    {
        public CtkTimeSecond? Time;//當次時間
        public CtkTimeSecond? PrevTime;//前一次時間
        public SNetTdTSignalSetSecF8 TSignal;//完整訊號
        public SNetTdTSignalSetSecF8 NewTSignal = new SNetTdTSignalSetSecF8();//新增訊號

    }
}
