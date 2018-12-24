using SensingNet.v0_1.Dsp.TimeSignal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.Dsp.Block
{
    public class SNetDspBlockFft : SNetDspBlockBase
    {
        public int PurgeSeconds = 60;
        public SNetDspTimeSignalSetSecond TSignalMin = new SNetDspTimeSignalSetSecond();
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



        }
      



    }
}
