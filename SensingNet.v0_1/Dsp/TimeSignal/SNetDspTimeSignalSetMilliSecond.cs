using CToolkit.v0_1.Timing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.Dsp.TimeSignal
{
    public class SNetDspTimeSignalSetMilliSecond : ISNetDspTimeSignalSet<CtkTimeMilliSecond, double>
    {
        public SortedDictionary<CtkTimeMilliSecond, List<double>> Signals = new SortedDictionary<CtkTimeMilliSecond, List<double>>();



        public KeyValuePair<CtkTimeMilliSecond, List<double>>? GetLastOrDefault()
        {
            if (this.Signals.Count == 0) return null;
            return this.Signals.Last();
        }


        #region ISNetDspTimeSignalSet

        public void AddByKey(CtkTimeMilliSecond key, IEnumerable<double> signals)
        {
            var list = this.GetOrCreate(key);
            list.AddRange(signals);
        }
        public bool ContainKey(CtkTimeMilliSecond key) { return this.Signals.ContainsKey((CtkTimeMilliSecond)key); }
        public List<double> GetOrCreate(CtkTimeMilliSecond key)
        {
            var k = (CtkTimeMilliSecond)key;
            if (!this.Signals.ContainsKey(k)) this.Signals[k] = new List<double>();
            return this.Signals[k];
        }
        public bool GetOrCreate(CtkTimeMilliSecond key, ref List<double> data)
        {
            var k = (CtkTimeMilliSecond)key;
            if (!this.Signals.ContainsKey(k))
            {
                data = new List<double>();
                this.Signals[k] = data;

                return true;
            }

            data = this.Signals[k];
            return false;
        }
        public void Set(CtkTimeMilliSecond key, List<double> signals)
        {
            var k = (CtkTimeMilliSecond)key;
            this.Signals[k] = signals;
        }
        public void Set(CtkTimeMilliSecond key, double signal)
        {
            var list = this.GetOrCreate(key);
            list.Clear();
            list.Add(signal);
        }

        #endregion

    }
}
