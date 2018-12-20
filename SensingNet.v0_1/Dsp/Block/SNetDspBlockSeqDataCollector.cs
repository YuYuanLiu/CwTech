using CToolkit.v0_1.TimeOp;
using MathNet.Numerics.LinearAlgebra.Double;
using SensingNet.v0_1.Dsp.TimeSignal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.Dsp.Block
{
    public class SNetDspBlockSeqDataCollector : SNetDspBlockBase
    {
        public SNetDspTimeSignalSetSecond DataSource = new SNetDspTimeSignalSetSecond();
        public int PurgeSeconds = 60;
        ~SNetDspBlockSeqDataCollector() { this.Dispose(false); }


        public SNetDspTimeSignalSetSecond GetOutput()
        {
            return this.DataSource;
        }

        public void Input(IEnumerable<double> vals, DateTime? time = null)
        {
            var now = DateTime.Now;
            var dt = now;
            if (time.HasValue) dt = time.Value;

            var key = new CtkTimeSecond(dt);
            var datas = this.DataSource.GetOrCreate(key);
            datas.AddRange(vals);


            var oldKey = new CtkTimeSecond(now.AddSeconds(-this.PurgeSeconds));
            var query = this.DataSource.Signals.Where(x => x.Key < oldKey).ToList();
            foreach (var ok in query)
                this.DataSource.Signals.Remove(ok.Key);

        }



        #region Event
        public event EventHandler<SNetDspBlockTimeSignalSetSecondEventArg> evtDataChange;

        protected void OnDataChange(SNetDspBlockTimeSignalSetSecondEventArg ea)
        {
            if (this.evtDataChange == null) return;
            this.evtDataChange(this, ea);
        }
        #endregion

    }


}

