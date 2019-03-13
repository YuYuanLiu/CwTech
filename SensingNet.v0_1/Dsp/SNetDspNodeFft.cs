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
using System.Threading.Tasks;

namespace SensingNet.v0_1.Dsp
{
    public class SNetDspNodeFft : SNetDspNodeF8
    {
        public int SampleRate = 1024;
        /// <summary>
        /// MathNet FFT 選 Matlab -> 算出來的結果可以加總後取平均, 仍是頻域圖
        /// </summary>
        public SNetDspTSignalSetSecF8 TSignal = new SNetDspTSignalSetSecF8();




        protected override void PurgeSignal()
        {
            if (this.PurgeSeconds <= 0) return;
            var now = DateTime.Now;
            var oldKey = new CtkTimeSecond(now.AddSeconds(-this.PurgeSeconds));
            this.PurgeSignalByTime(this.TSignal, oldKey);
        }

        public  void DoInput(object sender, SNetDspSignalSetSecF8EventArg ea)
        {
            if (!this.IsEnalbed) return;


            if (!ea.PrevTime.HasValue) return;
            if (ea.Time == ea.PrevTime.Value) return;
            var t = ea.PrevTime.Value;

            //取得時間變更前的時間資料
            IList<double> signalData = ea.TSignal.GetOrCreate(t);
            signalData = CtkNumUtil.InterpolationForce(signalData, this.SampleRate);

            var ctkNumContext = CtkNumContext.GetOrCreate();
            var comp = ctkNumContext.FftForward(signalData);

            var fftData = new double[comp.Length];
            this.TSignal.Set(t, fftData.ToList());

            Parallel.For(0, comp.Length, (idx) =>
            {
                fftData[idx] = comp[idx].Magnitude;
            });


            this.DoDataChange(this.TSignal, new SNetDspTSignalSecF8(t, signalData));
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
