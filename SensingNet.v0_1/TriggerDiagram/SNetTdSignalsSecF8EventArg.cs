using CToolkit.v1_0.Timing;
using SensingNet.v0_1.TimeSignal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.TriggerDiagram
{
    public class SNetTdSignalSecSetF8EventArg : SNetTdSignalEventArg
    {
        public CtkTimeSecond? Time;//當次時間
        public CtkTimeSecond? PrevTime;//前一次時間
        public SNetTdTSignalsSecF8 TSignalSource;//完整訊號來源
        public SNetTdTSignalsSecF8 TSignalNew = new SNetTdTSignalsSecF8();//此次新增訊號

        public SNetTdTSignalSecF8 GetThisOrLast()
        {
            if (this.Time.HasValue) return this.TSignalSource.Get(this.Time.Value);
            return this.TSignalSource.GetLastOrDefault();
        }

      

    }
}
