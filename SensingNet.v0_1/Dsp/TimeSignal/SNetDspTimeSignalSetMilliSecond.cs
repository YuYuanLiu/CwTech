using CToolkit.v0_1.TimeOp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.Dsp.TimeSignal
{
    public class SNetDspTimeSignalSetMilliSecond : ISNetDspTimeSignalSet
    {
        public SortedDictionary<CtkTimeMilliSecond, List<double>> Signals = new SortedDictionary<CtkTimeMilliSecond, List<double>>();

        public List<double> GetOrCreate(object key)
        {
            var k = (CtkTimeMilliSecond)key;
            if (!this.Signals.ContainsKey(k)) this.Signals[k] = new List<double>();
            return this.Signals[k];
        }

    }
}
