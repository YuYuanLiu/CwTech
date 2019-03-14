using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.TriggerDiagram.Basic
{
    public interface ISNetTdNode
    {

        String SNetDspIdentifier { get; set; }
        String SNetDspName { get; set; }

    }
}
