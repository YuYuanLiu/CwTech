using CToolkit.v0_1.TimeOp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.Dsp.TimeSignal
{
    public class SNetDspTimeSignalSecond : ISNetDspTimeSignal
    {
        //1 Ticks是100奈秒, 0 tick={0001/1/1 上午 12:00:00}
        //請勿使用Datetime, 避免有人誤解 比對只進行 年月日時分秒, 事實會比較到tick
        public SortedDictionary<CtkTimeSecond, double> Signals = new SortedDictionary<CtkTimeSecond, double>();

        public double GetOrCreate(object key)
        {
            var k = (CtkTimeSecond)key;
            if (!this.Signals.ContainsKey(k)) this.Signals[k] = 0.0;
            return this.Signals[k];
        }

        public void Set(object key, double val)
        {
            var k = (CtkTimeSecond)key;
            this.Signals[k] = val;
        }


    }
}
