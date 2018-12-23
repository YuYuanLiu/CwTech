using CToolkit.v0_1.TimeOp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.Dsp.TimeSignal
{
    public class SNetDspTimeSignalSetSecond : ISNetDspTimeSignalSet
    {
        //1 Ticks是100奈秒, 0 tick={0001/1/1 上午 12:00:00}
        //請勿使用Datetime, 避免有人誤解 比對只進行 年月日時分秒, 事實會比較到tick
        public SortedDictionary<CtkTimeSecond, List<double>> Signals = new SortedDictionary<CtkTimeSecond, List<double>>();

        public List<double> GetOrCreate(object key)
        {
            var k = (CtkTimeSecond)key;
            if (!this.Signals.ContainsKey(k)) this.Signals[k] = new List<double>();
            return this.Signals[k];
        }

        public KeyValuePair<CtkTimeSecond, List<double>>? GetLastOrDefault()
        {
            if (this.Signals.Count == 0) return null;
            return this.Signals.Last();
        }
    }
}
