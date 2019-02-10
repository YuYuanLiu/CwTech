using CToolkit.v0_1;
using CToolkit.v0_1.Timing;
using SensingNet.v0_1.Dsp.TimeSignal;
using System;
using System.Collections.Generic;

namespace SensingNet.v0_1.Dsp.Block
{

    public class SNetDspBlockSeqDataCollector : SNetDspBlock
    {
        public SNetDspTimeSignalSetSecond TSignal = new SNetDspTimeSignalSetSecond();

        ~SNetDspBlockSeqDataCollector() { this.Dispose(false); }


        public SNetDspTimeSignalSetSecond GetOutput()
        {
            return this.TSignal;
        }

        /// <summary>
        /// 建議照時間序列(Seq)來input, 避免後續使用的Block認定是Sequence, 然而不是
        /// </summary>
        /// <param name="vals"></param>
        /// <param name="dt"></param>
        public void Input(IEnumerable<double> vals, DateTime? dt = null)
        {
            if (!this.IsEnalbed) return;
            var now = DateTime.Now;
            var time = now;
            if (dt.HasValue) time = dt.Value;

            this.DoDataChange(this.TSignal, time, vals);
        }

        protected override void PurgeSignal()
        {
            if (this.PurgeSeconds <= 0) return;
            var now = DateTime.Now;
            var oldKey = new CtkTimeSecond(now.AddSeconds(-this.PurgeSeconds));
            this.PurgeSignalByTime(this.TSignal, oldKey);
        }





        #region IDisposable

        protected override void DisposeSelf()
        {
            CtkEventUtil.RemoveEventHandlersFromOwningByFilter( this, (dlgt) => true);//移除自己的Event Delegate
        }

        #endregion
    }


}

