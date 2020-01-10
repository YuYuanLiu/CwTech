using CToolkit.v1_0;
using CToolkit.v1_0.Timing;
using SensingNet.v0_2.TriggerDiagram.Basic;
using SensingNet.v0_2.TimeSignal;
using System;
using System.Collections.Generic;

namespace SensingNet.v0_2.TriggerDiagram
{

    public class SNetTdNSeqDataCollector : SNetTdNodeF8
    {
        public SNetTSignalSetSecF8 TSignal = new SNetTSignalSetSecF8();

        ~SNetTdNSeqDataCollector() { this.Dispose(false); }




        /// <summary>
        /// 建議照時間序列(Seq)來input, 避免後續使用的Block認定是Sequence, 然而不是
        /// </summary>
        /// <param name="vals"></param>
        /// <param name="dt"></param>
        public void Input(object sender, SNetTdSignalSetSecF8EventArg ea)
        {
            //IEnumerable<double> vals, DateTime? dt = null
            if (!this.IsEnalbed) return;
            foreach (var kv in ea.TSignalNew.Signals)
                this.DoInput(this.TSignal, kv);
        }
        public void Input(object sender, SNetTdSignalEventArg ea)
        {
            var myea = ea as SNetTdSignalSetSecF8EventArg;
            if (myea == null) throw new ArgumentException("不支援的參數類型");

            this.Input(sender, myea);
        }


        public void DoInput(object sender, SNetTdSignalSecF8EventArg ea)
        {
            //IEnumerable<double> vals, DateTime? dt = null
            if (!this.IsEnalbed) return;
            this.ProcDataInput(this.TSignal, ea.TSignal);
        }

        public void DoInput(object sender, SNetTdSignalEventArg ea)
        {
            var myea = ea as SNetTdSignalSecF8EventArg;
            if (myea == null) throw new ArgumentException("不支援的參數類型");

            this.DoInput(sender, myea);
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

