using CToolkit.v1_0.Timing;
using SensingNet.v0_1.TimeSignal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.TriggerDiagram
{
    public class SNetTdSignalSecF8EventArg : SNetTdSignalEventArg
    {
        public SNetTdTSignalSecF8 TSignal;



        public static implicit operator SNetTdSignalSecF8EventArg(SNetTdTSignalSecF8 val)
        {
            var rs = new SNetTdSignalSecF8EventArg();
            rs.TSignal = val;
            return rs;
        }

        public static implicit operator SNetTdSignalSecF8EventArg(KeyValuePair<CtkTimeSecond, List<double>> val)
        {
            var rs = new SNetTdSignalSecF8EventArg();
            rs.TSignal = val;
            return rs;
        }

    }
}
