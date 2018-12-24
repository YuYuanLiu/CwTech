using CToolkit.v0_1;
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
        public int PurgeSeconds = 60;
        public SNetDspTimeSignalSecond TSignalAvg = new SNetDspTimeSignalSecond();
        public SNetDspTimeSignalSecond TSignalMax = new SNetDspTimeSignalSecond();
        public SNetDspTimeSignalSecond TSignalMin = new SNetDspTimeSignalSecond();
        protected SNetDspBlockSeqDataCollector _input;
        public SNetDspBlockSeqDataCollector Input
        {
            get { return this._input; }
            set
            {
                //CtkEventUtil.RemoveEventHandlers(this._input, this);//input2 可能也來自同一個source
                this._input.evtDataChange -= _input_evtDataChange;
                this._input = value;
                this._input.evtDataChange += _input_evtDataChange;
            }
        }


        private void _input_evtDataChange(object sender, SNetDspBlockTimeSignalSetSecondEventArg e)
        {
            var key = e.Time;
            var list = e.TimeSignal.GetOrCreate(key);
            this.TSignalAvg.Set(e.Time, list.Average());
            this.TSignalMax.Set(e.Time, list.Max());
            this.TSignalMin.Set(e.Time, list.Min());

            var oldTime = DateTime.Now.AddSeconds(-this.PurgeSeconds);
            this.TSignalAvg.RemoveOldByTime(oldTime);
            this.TSignalMax.RemoveOldByTime(oldTime);
            this.TSignalMin.RemoveOldByTime(oldTime);


            e.InvokeResult = this.disposed ? SNetDspEnumInvokeResult.IsDisposed : SNetDspEnumInvokeResult.None;
        }

        ~SNetDspBlockBasicStatistics() { this.Dispose(false); }


        #region IDisposable
        // Flag: Has Dispose already been called?
        new bool disposed = false;

        // Public implementation of Dispose pattern callable by consumers.
        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
                //
                this.DisposeManaged();
            }

            // Free any unmanaged objects here.
            //
            this.DisposeUnmanaged();
            this.DisposeSelf();
            disposed = true;
        }



        protected override void DisposeManaged()
        {
        }
        protected override void DisposeSelf()
        {
            CtkEventUtil.RemoveEventHandlersFrom((dlgt) => true, this);
            CtkEventUtil.RemoveEventHandlers(this._input, this);

        }

        protected override void DisposeUnmanaged()
        {

        }
        #endregion
    }
}
