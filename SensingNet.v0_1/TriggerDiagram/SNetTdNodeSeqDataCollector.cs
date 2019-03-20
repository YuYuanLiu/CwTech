﻿using CToolkit.v1_0;
using CToolkit.v1_0.Timing;
using SensingNet.v0_1.TriggerDiagram.Basic;
using SensingNet.v0_1.TriggerDiagram.TimeSignal;
using System;
using System.Collections.Generic;

namespace SensingNet.v0_1.TriggerDiagram
{

    public class SNetTdNodeSeqDataCollector : SNetTdNodeF8
    {
        public SNetTdTSignalSetSecF8 TSignal = new SNetTdTSignalSetSecF8();

        ~SNetTdNodeSeqDataCollector() { this.Dispose(false); }




        /// <summary>
        /// 建議照時間序列(Seq)來input, 避免後續使用的Block認定是Sequence, 然而不是
        /// </summary>
        /// <param name="vals"></param>
        /// <param name="dt"></param>
        public void DoInput(object sender, SNetTdSignalSetSecF8EventArg ea)
        {
            //IEnumerable<double> vals, DateTime? dt = null
            if (!this.IsEnalbed) return;
            foreach (var kv in ea.NewTSignal.Signals)
                this.DoInput(this.TSignal, kv);
        }

        public void DoInput(object sender, SNetTdSignalSecF8EventArg ea)
        {
            //IEnumerable<double> vals, DateTime? dt = null
            if (!this.IsEnalbed) return;
            this.DoDataChange(this.TSignal, ea);
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
            base.DisposeSelf();
        }

        #endregion
    }


}

