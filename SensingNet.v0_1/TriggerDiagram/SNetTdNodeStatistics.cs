using CToolkit.v1_0;
using CToolkit.v1_0.Timing;
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

        public SNetTdTSignalSecSetF8 TSignalAvg = new SNetTdTSignalSecSetF8();
        public SNetTdTSignalSecSetF8 TSignalMax = new SNetTdTSignalSecSetF8();
        public SNetTdTSignalSecSetF8 TSignalMin = new SNetTdTSignalSecSetF8();


        ~SNetTdNodeStatistics() { this.Dispose(false); }



        protected override void Purge()
        {
            if (this.PurgeSeconds <= 0) return;
            var now = DateTime.Now;
            var oldKey = new CtkTimeSecond(now.AddSeconds(-this.PurgeSeconds));

            PurgeSignalByTime(this.TSignalAvg, oldKey);
            PurgeSignalByTime(this.TSignalMax, oldKey);
            PurgeSignalByTime(this.TSignalMin, oldKey);
        }


        public void DoInput(object sender, SNetTdSignalEventArg e)
        {
            if (!this.IsEnalbed) return;
            var ea = e as SNetTdSignalSecSetF8EventArg;
            if (ea == null) throw new SNetException("尚未無法處理此類資料: " + e.GetType().FullName);


            var key = ea.Time;
            var list = ea.TSignalSource.GetOrCreate(key.Value);
            this.TSignalAvg.Set(ea.Time.Value, list.Average());
            this.TSignalMax.Set(ea.Time.Value, list.Max());
            this.TSignalMin.Set(ea.Time.Value, list.Min());


            this.Purge();
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
