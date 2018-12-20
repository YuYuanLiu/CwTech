using SensingNet.v0_1.Dsp.TimeSignal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.Dsp.Block
{
    public class SNetDspBlockBasicStatistics : SNetDspBlockBase
    {
        public SNetDspTimeSignalSecond TSignalAvg = new SNetDspTimeSignalSecond();
        public SNetDspTimeSignalSecond TSignalMax = new SNetDspTimeSignalSecond();
        public SNetDspTimeSignalSecond TSignalMin = new SNetDspTimeSignalSecond();
        protected SNetDspBlockSeqDataCollector _input;
        public SNetDspBlockSeqDataCollector Input
        {
            get { return this._input; }
            set
            {
                if (this._input != null) this._input.evtDataChange -= _input_evtDataChange;
                this._input = value;
                this._input.evtDataChange += _input_evtDataChange;
            }
        }


        private void _input_evtDataChange(object sender, SNetDspBlockTimeSignalSetSecondEventArg e)
        {
            var key = e.Time;
            var list = e.TimeSignal.GetOrCreate(key);
            this.TSignalAvg.Set(e.Time, list.Average());
            this.TSignalMax.Set(e.Time, list.Max());
            this.TSignalMin.Set(e.Time, list.Min());
        }
      



    }
}
