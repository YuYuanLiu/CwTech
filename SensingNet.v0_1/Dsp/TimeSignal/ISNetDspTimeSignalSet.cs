using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.Dsp.TimeSignal
{
    public interface ISNetDspTimeSignalSet
    {
        void AddRange(object key, IEnumerable<double> signals);

        bool ContainKey(object key);

        List<double> GetOrCreate(object key);

        void Set(object key, List<double> signals);
        void Set(object key, double signal);

    }
}
