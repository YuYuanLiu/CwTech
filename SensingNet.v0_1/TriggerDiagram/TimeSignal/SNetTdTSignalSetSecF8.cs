using CToolkit.v0_1.Numeric;
using CToolkit.v0_1.Timing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.TriggerDiagram.TimeSignal
{
    public class SNetTdTSignalSetSecF8 : ISNetTdTSignalSet<CtkTimeSecond, double>
    {
        //1 Ticks是100奈秒, 0 tick={0001/1/1 上午 12:00:00}
        //請勿使用Datetime, 避免有人誤解 比對只進行 年月日時分秒, 事實會比較到tick
        public SortedDictionary<CtkTimeSecond, List<double>> Signals = new SortedDictionary<CtkTimeSecond, List<double>>();
        public List<double> this[CtkTimeSecond key] { get { return this.Signals[key]; } set { this.Signals[key] = value; } }

        public KeyValuePair<CtkTimeSecond, List<double>>? GetLastOrDefault()
        {
            if (this.Signals.Count == 0) return null;
            lock (this)
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

        public void AddByKey(CtkTimeSecond key, IEnumerable<double> signals)
        {
            var list = this.GetOrCreate(key);
            list.AddRange(signals);
        }
        public bool ContainKey(CtkTimeSecond key) { return this.Signals.ContainsKey((CtkTimeSecond)key); }
        public List<double> GetOrCreate(CtkTimeSecond key)
        {
            var k = (CtkTimeSecond)key;
            if (!this.Signals.ContainsKey(k))
            {
                lock (this)
                    this.Signals[k] = new List<double>();
            }
            return this.Signals[k];
        }
        public bool GetOrCreate(CtkTimeSecond key, ref List<double> data)
        {
            var k = (CtkTimeSecond)key;
            if (!this.Signals.ContainsKey(k))
            {
                data = new List<double>();
                lock (this)
                    this.Signals[k] = data;

                return true;
            }

            data = this.Signals[k];
            return false;
        }
        public void Set(CtkTimeSecond key, List<double> signals)
        {
            var k = (CtkTimeSecond)key;
            lock (this)
                this.Signals[k] = signals;
        }
        public void Set(CtkTimeSecond key, double signal)
        {
            var list = this.GetOrCreate(key);
            list.Clear();
            list.Add(signal);
        }

        #endregion



        public static implicit operator SNetTdTSignalSetSecF8(SNetTdTSignalSecF8 val)
        {
            var rs = new SNetTdTSignalSetSecF8();
            if (!val.Time.HasValue) throw new ArgumentException("Time can not be null");
            rs.Signals[val.Time.Value] = val.Signals;
            return rs;
        }
    }
}
