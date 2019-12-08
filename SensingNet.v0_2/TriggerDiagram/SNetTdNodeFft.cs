using CToolkit.v1_0;
using CToolkit.v1_0.Numeric;
using CToolkit.v1_0.Timing;
using MathNet.Filtering.FIR;
using SensingNet.v0_2.TriggerDiagram.Basic;
using SensingNet.v0_2.TimeSignal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensingNet.v0_2.TriggerDiagram
{
    public class SNetTdNodeFft : SNetTdNodeF8
    {
        public int SampleRate = 1024;
        /// <summary>
        /// MathNet FFT 選 Matlab -> 算出來的結果可以加總後取平均, 仍是頻域圖
        /// </summary>
        public SNetTSignalsSecF8 TSignal = new SNetTSignalsSecF8();




        protected override void Purge()
        {
            if (this.PurgeSeconds <= 0) return;
            var now = DateTime.Now;
            var oldKey = new CtkTimeSecond(now.AddSeconds(-this.PurgeSeconds));
            PurgeSignalByTime(this.TSignal, oldKey);
        }

        public void Input(object sender, SNetTdEventArg e)
        {
            if (!this.IsEnalbed) return;
            var ea = e as SNetTdSignalsSecF8EventArg;
            if (ea == null) throw new SNetException("尚未無法處理此類資料: " + e.GetType().FullName);




            if (!ea.PrevTime.HasValue) return;
            if (ea.Time == ea.PrevTime.Value) return;
            var t = ea.PrevTime.Value;

            //取得時間變更前的時間資料
            IList<double> signalData = ea.TSignalSource.GetOrCreate(t);
            signalData = CtkNumUtil.InterpolationForce(signalData, this.SampleRate);

            var ctkNumContext = CtkNumContext.GetOrCreate();
            var comp = ctkNumContext.FftForward(signalData);

            var fftData = new double[comp.Length];
            this.TSignal.Set(t, fftData.ToList());

            Parallel.For(0, comp.Length, (idx) =>
            {
                fftData[idx] = comp[idx].Magnitude;
            });


            this.ProcDataInput(this.TSignal, new SNetTSignalSecF8(t, signalData));
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
