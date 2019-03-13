using CToolkit.v0_1;
using CToolkit.v0_1.Numeric;
using CToolkit.v0_1.Timing;
using MathNet.Filtering.FIR;
using SensingNet.v0_1.Dsp.TimeSignal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensingNet.v0_1.Dsp.Old
{
    public class SNetDspNodeFft : SNetDspBlock
    {
        public int SampleRate = 1024;
        /// <summary>
        /// MathNet FFT 選 Matlab -> 算出來的結果可以加總後取平均, 仍是頻域圖
        /// </summary>
        public SNetDspTimeSignalSetSecond TSignal = new SNetDspTimeSignalSetSecond();

        protected SNetDspBlock _input;


        public SNetDspBlock Input
        {
            get { return this._input; }
            set
            {
                if (this._input != null) this._input.evtDataChange -= _input_evtDataChange;
                this._input = value;
                this._input.evtDataChange += _input_evtDataChange;
            }
        }


        protected override void PurgeSignal()
        {
            if (this.PurgeSeconds <= 0) return;
            var now = DateTime.Now;
            var oldKey = new CtkTimeSecond(now.AddSeconds(-this.PurgeSeconds));
            this.PurgeSignalByTime(this.TSignal, oldKey);
        }

        private void _input_evtDataChange(object sender, SNetDspTimeSignalEventArg e)
        {
            if (!this.IsEnalbed) return;
            var ea = e as SNetDspTimeSignalSetSecondEventArg;
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


            this.DoDataChange(this.TSignal, t, signalData);
            e.InvokeResult = this.disposed ? SNetDspEnumInvokeResult.IsDisposed : SNetDspEnumInvokeResult.None;
        }
        #region IDisposable

        protected override void DisposeSelf()
        {
            CtkEventUtil.RemoveEventHandlersFromOwningByFilter(this, (dlgt) => true);//移除自己的Event Delegate
            CtkEventUtil.RemoveEventHandlersFromOwningByTarget(this._input, this);//移除在別人那的Event Delegate
        }

        #endregion


    }
}
