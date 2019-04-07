using CToolkit.v1_0;
using CToolkit.v1_0.Timing;
using SensingNet.v0_1.TriggerDiagram.Basic;
using SensingNet.v0_1.TriggerDiagram.TimeSignal;
using System;
using System.Collections.Generic;

namespace SensingNet.v0_1.TriggerDiagram
{

    public class SNetTdNodeSeqDataCollector : SNetTdNodeF8
    {
        public SNetTdTSignalSecSetF8 TSignal = new SNetTdTSignalSecSetF8();

        ~SNetTdNodeSeqDataCollector() { this.Dispose(false); }




        /// <summary>
        /// 建議照時間序列(Seq)來input, 避免後續使用的Block認定是Sequence, 然而不是
        /// </summary>
        /// <param name="vals"></param>
        /// <param name="dt"></param>
        public void DoInput(object sender, SNetTdSignalSecSetF8EventArg ea)
        {
            //IEnumerable<double> vals, DateTime? dt = null
            if (!this.IsEnalbed) return;
            foreach (var kv in ea.TSignalNew.Signals)
                this.DoInput(this.TSignal, kv);
        }

        public void DoInput(object sender, SNetTdSignalSecF8EventArg ea)
        {
            //IEnumerable<double> vals, DateTime? dt = null
            if (!this.IsEnalbed) return;
            this.ProcDataInput(this.TSignal, ea.TSignal);
        }


        protected override void Purge()
        {
            if (this.PurgeSeconds <= 0) return;
            var now = DateTime.Now;
            var oldKey = new CtkTimeSecond(now.AddSeconds(-this.PurgeSeconds));
            PurgeSignalByTime(this.TSignal, oldKey);
        }





        #region IDisposable

        protected override void DisposeSelf()
        {
            base.DisposeSelf();
        }

        #endregion
    }


}

