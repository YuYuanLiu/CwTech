using CToolkit.v1_0;
using CToolkit.v1_0.Numeric;
using CToolkit.v1_0.Timing;
using MathNet.Filtering.FIR;
using SensingNet.v0_1.TriggerDiagram.Basic;
using SensingNet.v0_1.TriggerDiagram.TimeSignal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensingNet.v0_1.TriggerDiagram
{
    public class SNetTdNodeFft : SNetTdNodeF8
    {
        public int SampleRate = 1024;
        /// <summary>
        /// MathNet FFT 選 Matlab -> 算出來的結果可以加總後取平均, 仍是頻域圖
        /// </summary>
        public SNetTdTSignalSetSecF8 TSignal = new SNetTdTSignalSetSecF8();




        protected override void PurgeSignal()
        {
            if (this.PurgeSeconds <= 0) return;
            var now = DateTime.Now;
            var oldKey = new CtkTimeSecond(now.AddSeconds(-this.PurgeSeconds));
            this.PurgeSignalByTime(this.TSignal, oldKey);
        }

        public  void DoInput(object sender, SNetTdEventArg e)
        {
            if (!this.IsEnalbed) return;
            var ea = e as SNetTdSignalSetSecF8EventArg;
            if (ea == null) throw new SNetException("尚未無法處理此類資料: " + e.GetType().FullName);




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


            this.DoDataChange(this.TSignal, new SNetTdTSignalSecF8(t, signalData));
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
