using CToolkit.v0_1.Timing;
using SensingNet.v0_1.Dsp.TimeSignal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.Dsp.Basic
{
    public class SNetDspEventArg : EventArgs
    {
        public SNetDspEnumInvokeResult InvokeResult = SNetDspEnumInvokeResult.None;



        public static implicit operator SNetDspEventArg(SNetDspEnumInvokeResult result) { return new SNetDspEventArg() { InvokeResult = result }; }
    }
}
