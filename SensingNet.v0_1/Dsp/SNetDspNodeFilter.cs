using CToolkit.v0_1;
using CToolkit.v0_1.Numeric;
using CToolkit.v0_1.Timing;
using MathNet.Filtering.FIR;
using SensingNet.v0_1.Dsp.Basic;
using SensingNet.v0_1.Dsp.TimeSignal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.Dsp
{
    public class SNetDspNodeFilter : SNetDspNodeF8
    {
        //使用Struct傳入是傳值, 修改是無法帶出來的, 但你可以回傳同一個結構後接住它
        public CtkPassFilterStruct FilterArgs = new CtkPassFilterStruct()
        {
            CutoffHigh = 512,
            CutoffLow = 5,
            Mode = CtkEnumPassFilterMode.None,
            SampleRate = 1024,
        };

        public CtkFftOnlineFilter PassFilter = new CtkFftOnlineFilter();
        public SNetDspTSignalSetSecF8 TSignal = new SNetDspTSignalSetSecF8();

        public void DoInput(object sender, SNetDspSignalEventArg e)
        {
            if (!this.IsEnalbed) return;
            var tsSetSecondEa = e as SNetDspSignalSetSecF8EventArg;
            if (tsSetSecondEa == null) throw new SNetException("尚未無法處理此類資料: " + e.GetType().FullName);


            if (!tsSetSecondEa.PrevTime.HasValue) return;
            if (tsSetSecondEa.Time == tsSetSecondEa.PrevTime.Value) return;
            var t = tsSetSecondEa.PrevTime.Value;

            //取得時間變更前的時間資料
            IList<double> signalData = tsSetSecondEa.TSignal.GetOrCreate(t);


            if (this.FilterArgs.Mode != CtkEnumPassFilterMode.None)
            {
                this.PassFilter.SetFilter(this.FilterArgs);
                signalData = CtkNumUtil.InterpolationCanOneOrZero(signalData, (int)this.FilterArgs.SampleRate);
                signalData = this.PassFilter.ProcessSamples(signalData);
            }

            this.DoDataChange(this.TSignal, new SNetDspTSignalSecF8(t, signalData));
            e.InvokeResult = this.disposed ? SNetDspEnumInvokeResult.IsDisposed : SNetDspEnumInvokeResult.None;
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
