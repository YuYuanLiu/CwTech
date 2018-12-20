using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.Dsp.TimeSignal
{
    public interface ISNetDspTimeSignalSet
    {
        List<double> GetOrCreate(object key);
    }
}
