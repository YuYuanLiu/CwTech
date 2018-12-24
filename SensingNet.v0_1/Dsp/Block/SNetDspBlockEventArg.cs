using CToolkit.v0_1.TimeOp;
using SensingNet.v0_1.Dsp.TimeSignal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.Dsp.Block
{
    public class SNetDspBlockEventArg : EventArgs
    {
        public SNetDspEnumInvokeResult InvokeResult = SNetDspEnumInvokeResult.None;



        public static implicit operator SNetDspBlockEventArg(SNetDspEnumInvokeResult result) { return new SNetDspBlockEventArg() { InvokeResult = result }; }
    }
}
