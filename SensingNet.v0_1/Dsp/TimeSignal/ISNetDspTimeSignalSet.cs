using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.Dsp.TimeSignal
{
    public interface ISNetDspTimeSignalSet<T, S>
    {
        void AddByKey(T key, IEnumerable<S> signals);

        bool ContainKey(T key);

        List<S> GetOrCreate(T key);

        void Set(T key, List<S> signals);
        void Set(T key, S signals);

    }
}
