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

namespace SensingNet.v0_1.Dsp.Block
{
    public class SNetDspBlockFilter : SNetDspBlockBase
    {
        public CtkFftOnlineFilter PassFilter = new CtkFftOnlineFilter();
        //使用Struct傳入是傳值, 修改是無法帶出來的, 但你可以回傳同一個結構後接住它
        public CtkPassFilterStruct FilterArgs = new CtkPassFilterStruct()
        {
            CutoffHigh = 512,
            CutoffLow = 5,
            Mode = CtkEnumPassFilterMode.None,
            SampleRate = 1024,
        };
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
            if (!this.IsEnalbed) return;
            var tsSetSecondEa = e as SNetDspBlockTimeSignalSetSecondEventArg;
            if (tsSetSecondEa == null) throw new SNetException("尚未無法處理此類資料: " + e.GetType().FullName);


            if (!tsSetSecondEa.PrevTime.HasValue) return;
            if (tsSetSecondEa.Time == tsSetSecondEa.PrevTime.Value) return;
            var t = tsSetSecondEa.PrevTime.Value;

            //取得時間變更前的時間資料
            IEnumerable<double> signalData = tsSetSecondEa.TSignal.GetOrCreate(t);


            if (this.FilterArgs.Mode != CtkEnumPassFilterMode.None)
            {
                this.PassFilter.SetFilter(this.FilterArgs);
                signalData = CtkNumUtil.InterpolationCanOneOrZero(signalData, (int)this.FilterArgs.SampleRate);
                signalData = this.PassFilter.ProcessSamples(signalData);
            }

            this.DoDataChange(this.TSignal, t, signalData);
            e.InvokeResult = this.disposed ? SNetDspEnumInvokeResult.IsDisposed : SNetDspEnumInvokeResult.None;
        }
        #region IDisposable

        protected override void DisposeSelf()
        {
            CtkEventUtil.RemoveEventHandlersFromOwningByFilter( this, (dlgt) => true);//移除自己的Event Delegate
            CtkEventUtil.RemoveEventHandlersFromOwningByTarget(this._input, this);//移除在別人那的Event Delegate
        }

        #endregion


    }
}
