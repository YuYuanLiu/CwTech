using CToolkit.v1_0;
using CToolkit.v1_0.Timing;
using SensingNet.v0_2.TriggerDiagram.Basic;
using SensingNet.v0_2.TimeSignal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_2.TriggerDiagram
{
    public class SNetTdNStatistics : SNetTdNodeF8
    {

        public SNetTSignalSetSecF8 TSignalAvg = new SNetTSignalSetSecF8();
        public SNetTSignalSetSecF8 TSignalMax = new SNetTSignalSetSecF8();
        public SNetTSignalSetSecF8 TSignalMin = new SNetTSignalSetSecF8();


        ~SNetTdNStatistics() { this.Dispose(false); }



        protected override void Purge()
        {
            if (this.PurgeSeconds <= 0) return;
            var now = DateTime.Now;
            var oldKey = new CtkTimeSecond(now.AddSeconds(-this.PurgeSeconds));

            PurgeSignalByTime(this.TSignalAvg, oldKey);
            PurgeSignalByTime(this.TSignalMax, oldKey);
            PurgeSignalByTime(this.TSignalMin, oldKey);
        }


        public void Input(object sender, SNetTdSignalEventArg e)
        {
            if (!this.IsEnalbed) return;
            var ea = e as SNetTdSignalSetSecF8EventArg;
            if (ea == null) throw new SNetException("尚未無法處理此類資料: " + e.GetType().FullName);


            var key = ea.Time;
            var list = ea.TSignalSource.GetOrCreate(key.Value);
            this.TSignalAvg.Set(ea.Time.Value, list.Average());
            this.TSignalMax.Set(ea.Time.Value, list.Max());
            this.TSignalMin.Set(ea.Time.Value, list.Min());


            this.Purge();

            this.OnDataChange(ea);

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
