using CToolkit.v0_1;
using CToolkit.v0_1.Timing;
using SensingNet.v0_1.TriggerDiagram.Basic;
using SensingNet.v0_1.TriggerDiagram.TimeSignal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.TriggerDiagram
{
    public class SNetTdNodeStatistics : SNetTdNodeF8
    {

        public SNetTdTSignalSetSecF8 TSignalAvg = new SNetTdTSignalSetSecF8();
        public SNetTdTSignalSetSecF8 TSignalMax = new SNetTdTSignalSetSecF8();
        public SNetTdTSignalSetSecF8 TSignalMin = new SNetTdTSignalSetSecF8();


        ~SNetTdNodeStatistics() { this.Dispose(false); }



        protected override void PurgeSignal()
        {
            if (this.PurgeSeconds <= 0) return;
            var now = DateTime.Now;
            var oldKey = new CtkTimeSecond(now.AddSeconds(-this.PurgeSeconds));

            this.PurgeSignalByTime(this.TSignalAvg, oldKey);
            this.PurgeSignalByTime(this.TSignalMax, oldKey);
            this.PurgeSignalByTime(this.TSignalMin, oldKey);
        }


        public void DoInput(object sender, SNetTdSignalEventArg e)
        {
            if (!this.IsEnalbed) return;
            var ea = e as SNetTdSignalSetSecF8EventArg;
            if (ea == null) throw new SNetException("尚未無法處理此類資料: " + e.GetType().FullName);


            var key = ea.Time;
            var list = ea.TSignal.GetOrCreate(key.Value);
            this.TSignalAvg.Set(ea.Time.Value, list.Average());
            this.TSignalMax.Set(ea.Time.Value, list.Max());
            this.TSignalMin.Set(ea.Time.Value, list.Min());


            this.PurgeSignal();
            ea.InvokeResult = this.disposed ? SNetTdEnumInvokeResult.IsDisposed : SNetTdEnumInvokeResult.None;
        }


        #region IDisposable

        protected override void DisposeSelf()
        {
            base.DisposeSelf();
        }

        #endregion
    }
}
