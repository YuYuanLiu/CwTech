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
        /// 集合型, 建議照時間序列(Seq)來input, 避免後續使用的Block認定是Sequence, 然而不是
        /// </summary>
        /// <param name="vals"></param>
        /// <param name="dt"></param>
        public void TgInput(object sender, SNetTdSignalSetSecF8EventArg ea)
        {
            //IEnumerable<double> vals, DateTime? dt = null
            if (!this.IsEnalbed) return;
            foreach (var kv in ea.TSignalNew.Signals)
                this.TgInput(this.TSignal, kv);
        }

        /// <summary>
        /// 基底類別, 自動判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ea"></param>
        public void TgInput(object sender, SNetTdSignalEventArg ea)
        {
            //父類別進入, 先判斷有沒有支援
            var eaSingle = ea as SNetTdSignalSecF8EventArg;
            if (eaSingle != null)
            {
                this.TgInput(sender, eaSingle);
                return;
            }

            var eaSet = ea as SNetTdSignalSetSecF8EventArg;
            if (eaSet != null)
            {
                this.TgInput(sender, eaSet);
                return;
            }
        }

        /// <summary>
        /// 單一型, 直接執行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ea"></param>
        public void TgInput(object sender, SNetTdSignalSecF8EventArg ea)
        {
            if (!this.IsEnalbed) return;
            this.ProcAndPushData(this.TSignal, ea.TSignal);
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

