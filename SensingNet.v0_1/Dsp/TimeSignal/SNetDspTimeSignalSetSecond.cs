using CToolkit.v0_1.Numeric;
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
        public List<double> this[CtkTimeSecond key] { get { return this.Signals[key]; } set { this.Signals[key] = value; } }

        public KeyValuePair<CtkTimeSecond, List<double>>? GetLastOrDefault()
        {
            if (this.Signals.Count == 0) return null;
            return this.Signals.Last();
        }
        public void Interpolation(int dataSize)
        {
            foreach (var kv in this.Signals)
            {
                var ss = kv.Value;
                if (dataSize == ss.Count) continue;
                var list = CtkNumUtil.InterpolationCanOneOrZero(ss, dataSize);
                kv.Value.Clear();
                kv.Value.AddRange(list);
            }
        }

        #region ISNetDspTimeSignalSet

        public void AddByKey(object key, IEnumerable<double> signals)
        {
            var list = this.GetOrCreate(key);
            list.AddRange(signals);
        }
        public bool ContainKey(object key) { return this.Signals.ContainsKey((CtkTimeSecond)key); }
        public List<double> GetOrCreate(object key)
        {
            var k = (CtkTimeSecond)key;
            if (!this.Signals.ContainsKey(k)) this.Signals[k] = new List<double>();
            return this.Signals[k];
        }
        public bool GetOrCreate(object key, ref List<double> data)
        {
            var k = (CtkTimeSecond)key;
            if (!this.Signals.ContainsKey(k))
            {
                data = new List<double>();
                this.Signals[k] = data;

                return true;
            }

            data = this.Signals[k];
            return false;
        }
        public void Set(object key, List<double> signals)
        {
            var k = (CtkTimeSecond)key;
            this.Signals[k] = signals;
        }
        public void Set(object key, double signal)
        {
            var list = this.GetOrCreate(key);
            list.Clear();
            list.Add(signal);
        }

        #endregion

    }
}
