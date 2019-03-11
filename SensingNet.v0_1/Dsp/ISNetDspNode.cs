using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.Dsp
{
    public interface ISNetDspNode
    {
        String SNetDspIdentifier { get; set; }
        String SNetDspName { get; set; }

    }
}
