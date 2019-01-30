using CToolkit.v0_1;
using CToolkit.v0_1.Timing;
using SensingNet.v0_1.Dsp.TimeSignal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.Dsp.Block
{
    public class SNetDspBlockBasicStatistics : SNetDspBlockBase
    {

        public SNetDspTimeSignalSetSecond TSignalAvg = new SNetDspTimeSignalSetSecond();
        public SNetDspTimeSignalSetSecond TSignalMax = new SNetDspTimeSignalSetSecond();
        public SNetDspTimeSignalSetSecond TSignalMin = new SNetDspTimeSignalSetSecond();
        protected SNetDspBlockBase _input;
        ~SNetDspBlockBasicStatistics() { this.Dispose(false); }

        public SNetDspBlockBase Input
        {
            get { return this._input; }
            set
            {
                //注意, 被參照 (包含在別人的event裡加入delegate), 只有在對方被釋放時才會被釋放, 所以要 -=
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

            this.PurgeSignalByTime(this.TSignalAvg, oldKey);
            this.PurgeSignalByTime(this.TSignalMax, oldKey);
            this.PurgeSignalByTime(this.TSignalMin, oldKey);
        }

        private void _input_evtDataChange(object sender, SNetDspBlockTimeSignalEventArg e)
        {
            var tsSetSecondEa = e as SNetDspBlockTimeSignalSetSecondEventArg;
            if (tsSetSecondEa == null) throw new SNetException("尚無法處理此類資料: " + e.GetType().FullName);



            var key = tsSetSecondEa.Time;
            var list = tsSetSecondEa.TSignal.GetOrCreate(key);
            this.TSignalAvg.Set(tsSetSecondEa.Time, list.Average());
            this.TSignalMax.Set(tsSetSecondEa.Time, list.Max());
            this.TSignalMin.Set(tsSetSecondEa.Time, list.Min());





            this.PurgeSignal();


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
