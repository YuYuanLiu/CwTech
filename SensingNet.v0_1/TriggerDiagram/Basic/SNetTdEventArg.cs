using CToolkit.v0_1.Timing;
using SensingNet.v0_1.TriggerDiagram.TimeSignal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.TriggerDiagram.Basic
{
    public class SNetTdEventArg : EventArgs
    {
        public SNetTdEnumInvokeResult InvokeResult = SNetTdEnumInvokeResult.None;



        public static implicit operator SNetTdEventArg(SNetTdEnumInvokeResult result) { return new SNetTdEventArg() { InvokeResult = result }; }
    }
}
