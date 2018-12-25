using CToolkit.v0_1;
using CToolkit.v0_1.NumericProc;
using CToolkit.v0_1.TimeOp;
using MathNet.Filtering.FIR;
using SensingNet.v0_1.Dsp.TimeSignal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.Dsp.Block
{
    public class SNetDspBlockFilter : SNetDspBlockBase
    {
        public SNetDspBlockFilter_PassFilter PassFilter = new SNetDspBlockFilter_PassFilter();
        public int PassFilterCutoffHigh = 512;
        public int PassFilterCutoffLow = 5;
        public CtkEnumPassFilter PassFilterMode = CtkEnumPassFilter.None;
        public int PassFilterSampleRate = 1024;
        public SNetDspTimeSignalSetSecond TSignal = new SNetDspTimeSignalSetSecond();
        protected SNetDspBlockBase _input;


        public SNetDspBlockBase Input
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

        private void _input_evtDataChange(object sender, SNetDspBlockTimeSignalEventArg e)
        {
            var tsSetSecondEa = e as SNetDspBlockTimeSignalSetSecondEventArg;
            if (tsSetSecondEa == null) throw new SNetException("尚未無法處理此類資料: " + e.GetType().FullName);


            if (!tsSetSecondEa.BeforeLastTime.HasValue) return;
            if (tsSetSecondEa.Time == tsSetSecondEa.BeforeLastTime.Value) return;
            var t = tsSetSecondEa.BeforeLastTime.Value;

            //取得時間變更前的時間資料
            IEnumerable<double> signalData = tsSetSecondEa.TSignal.GetOrCreate(t);


            if (this.PassFilterMode != CtkEnumPassFilter.None)
            {
                this.PassFilter.InitFilterIfNull(this.PassFilterMode, this.PassFilterSampleRate, this.PassFilterCutoffLow, this.PassFilterCutoffHigh);
                signalData = CtkNpUtil.Interpolation(signalData, (int)this.PassFilterSampleRate);
                signalData = this.PassFilter.ProcessSamples(signalData);
            }

            this.DoDataChange(this.TSignal, t, signalData);
            e.InvokeResult = this.disposed ? SNetDspEnumInvokeResult.IsDisposed : SNetDspEnumInvokeResult.None;
        }
        #region IDisposable

        protected override void DisposeSelf()
        {
            CtkEventUtil.RemoveEventHandlersFrom((dlgt) => true, this);//移除自己的Event Delegate
            CtkEventUtil.RemoveEventHandlers(this._input, this);//移除在別人那的Event Delegate
        }

        #endregion


    }
}
