using CToolkit.v0_1.Numeric;
using CToolkit.v0_1.Timing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.Dsp.TimeSignal
{
    public class SNetDspTSignalSecF8 : ISNetDspTimeSignal<CtkTimeSecond, double>
    {
        //1 Ticks是100奈秒, 0 tick={0001/1/1 上午 12:00:00}
        //請勿使用Datetime, 避免有人誤解 比對只進行 年月日時分秒, 事實會比較到tick
        public CtkTimeSecond? Time;
        public List<double> Signals = new List<double>();

        public SNetDspTSignalSecF8() { }
        public SNetDspTSignalSecF8(CtkTimeSecond? time, IEnumerable<double> signals)
        {
            this.Time = time;
            this.Signals.AddRange(signals);

        }

        public static implicit operator SNetDspTSignalSecF8(KeyValuePair<CtkTimeSecond, List<double>> val) { return new SNetDspTSignalSecF8(val.Key, val.Value); }



    }
}
