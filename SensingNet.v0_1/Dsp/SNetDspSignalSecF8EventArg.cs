using CToolkit.v0_1.Timing;
using SensingNet.v0_1.Dsp.TimeSignal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.Dsp
{
    public class SNetDspSignalSecF8EventArg : SNetDspSignalEventArg
    {
        public SNetDspTSignalSecF8 TSignal;



        public static implicit operator SNetDspSignalSecF8EventArg(SNetDspTSignalSecF8 val)
        {
            var rs = new SNetDspSignalSecF8EventArg();
            rs.TSignal = val;
            return rs;
        }

        public static implicit operator SNetDspSignalSecF8EventArg(KeyValuePair<CtkTimeSecond, List<double>> val)
        {
            var rs = new SNetDspSignalSecF8EventArg();
            rs.TSignal = val;
            return rs;
        }

    }
}
