using CToolkit.v0_1;
using CToolkit.v0_1.Timing;
using SensingNet.v0_1.Dsp.Basic;
using SensingNet.v0_1.Dsp.TimeSignal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.Dsp
{
    public class SNetDspNodeStatistics : SNetDspNodeF8
    {

        public SNetDspTSignalSetSecF8 TSignalAvg = new SNetDspTSignalSetSecF8();
        public SNetDspTSignalSetSecF8 TSignalMax = new SNetDspTSignalSetSecF8();
        public SNetDspTSignalSetSecF8 TSignalMin = new SNetDspTSignalSetSecF8();


        ~SNetDspNodeStatistics() { this.Dispose(false); }



        protected override void PurgeSignal()
        {
            if (this.PurgeSeconds <= 0) return;
            var now = DateTime.Now;
            var oldKey = new CtkTimeSecond(now.AddSeconds(-this.PurgeSeconds));

            this.PurgeSignalByTime(this.TSignalAvg, oldKey);
            this.PurgeSignalByTime(this.TSignalMax, oldKey);
            this.PurgeSignalByTime(this.TSignalMin, oldKey);
        }


        public void DoInput(object sender, SNetDspSignalEventArg e)
        {
            if (!this.IsEnalbed) return;
            var ea = e as SNetDspSignalSetSecF8EventArg;
            if (ea == null) throw new SNetException("尚未無法處理此類資料: " + e.GetType().FullName);


            var key = ea.Time;
            var list = ea.TSignal.GetOrCreate(key.Value);
            this.TSignalAvg.Set(ea.Time.Value, list.Average());
            this.TSignalMax.Set(ea.Time.Value, list.Max());
            this.TSignalMin.Set(ea.Time.Value, list.Min());


            this.PurgeSignal();
            ea.InvokeResult = this.disposed ? SNetDspEnumInvokeResult.IsDisposed : SNetDspEnumInvokeResult.None;
        }


        #region IDisposable

        protected override void DisposeSelf()
        {
            base.DisposeSelf();
        }

        #endregion
    }
}
