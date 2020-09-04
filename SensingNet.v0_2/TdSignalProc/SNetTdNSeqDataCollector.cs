using CToolkit.v1_1;
using CToolkit.v1_1.Timing;
using SensingNet.v0_2.TimeSignal;
using System;
using System.Collections.Generic;

namespace SensingNet.v0_2.TdSignalProc
{

    public class SNetTdNSeqDataCollector : SNetTdNodeF8
    {
        public SNetTSignalSetSecF8 TSignalSet = new SNetTSignalSetSecF8();
        public bool IsTriggeredPerSecond = false;

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
                this.TgInput(this.TSignalSet, kv);
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
        /// 最後都會執行這段
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ea"></param>
        public void TgInput(object sender, SNetTdSignalSecF8EventArg ea)
        {
            if (!this.IsEnalbed) return;

            var tSignalSet = this.TSignalSet;
            var newSignals = ea.TSignal;
            var time = newSignals.Time.HasValue ? newSignals.Time.Value : DateTime.Now;


            tSignalSet.AddByKey(time, newSignals.Signals);
            var evtea = new SNetTdSignalSetSecF8EventArg()
            {
                Sender = this,
                Time = time,
                TSignalSource = tSignalSet,
                PrevTime = this.PrevTime,
            };

            if (this.IsTriggeredPerSecond)
            {
                if (this.PrevTime.HasValue && this.PrevTime != time)
                {
                    var prevTime = this.PrevTime.HasValue ? this.PrevTime.Value : DateTime.Now;
                    if (this.TSignalSet.ContainKey(prevTime))
                    {
                        var prevSignal = this.TSignalSet.Get(prevTime);
                        evtea.TSignalNew.AddByKey(time, newSignals.Signals);
                        this.OnDataChange(evtea);
                    }
                }
            }
            else
            {
                evtea.TSignalNew.AddByKey(time, newSignals.Signals);
                this.OnDataChange(evtea);
            }


            this.Purge();
            this.PrevTime = time;


        }



        protected override void Purge()
        {
            if (this.PurgeSeconds <= 0) return;
            var now = DateTime.Now;
            //var oldKey = new CtkTimeSecond(now.AddSeconds(-this.PurgeSeconds));
            //PurgeSignalByTime(this.TSignalSet, oldKey);
            PurgeSignalByCount(this.TSignalSet, this.PurgeSeconds);
        }





        #region IDisposable

        protected override void DisposeSelf()
        {
            base.DisposeSelf();
        }

        #endregion
    }


}

