﻿using CToolkit.v1_0.Numeric;
using CToolkit.v1_0.Timing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.TriggerDiagram.TimeSignal
{
    /// <summary>
    /// signal list at specified time
    /// </summary>
    public class SNetTdTSignalSecF8 : ISNetTdTSignal<CtkTimeSecond, double>
    {
        //1 Ticks是100奈秒, 0 tick={0001/1/1 上午 12:00:00}
        //請勿使用Datetime, 避免有人誤解 比對只進行 年月日時分秒, 事實會比較到tick
        public CtkTimeSecond? Time;
        public List<double> Signals = new List<double>();

        public SNetTdTSignalSecF8() { }
        public SNetTdTSignalSecF8(CtkTimeSecond? time, IEnumerable<double> signals)
        {
            this.Time = time;
            this.Signals.AddRange(signals);
        }




        #region Static Operator

        public static implicit operator SNetTdTSignalSecF8(KeyValuePair<CtkTimeSecond, List<double>> val) { return new SNetTdTSignalSecF8(val.Key, val.Value); }

        #endregion

    }
}
