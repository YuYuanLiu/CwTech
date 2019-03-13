using CToolkit.v0_1;
using CToolkit.v0_1.Timing;
using SensingNet.v0_1.Dsp.Basic;
using SensingNet.v0_1.Dsp.TimeSignal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.Dsp
{
    public class SNetDspNodeF8: SNetDspNode
    {

        public CtkTimeSecond? PrevTime;
        public int PurgeSeconds = 60;


        protected virtual void PurgeSignal()
        {
            throw new NotImplementedException();
        }
        protected virtual void PurgeSignalByCount(SNetDspTSignalSetSecF8 tSignal, int Count)
        {
            var query = tSignal.Signals.Take(tSignal.Signals.Count - Count).ToList();
            foreach (var ok in query)
                tSignal.Signals.Remove(ok.Key);
        }
        protected virtual void PurgeSignalByTime(SNetDspTSignalSetSecF8 tSignal, CtkTimeSecond time)
        {
            var now = DateTime.Now;
            var query = tSignal.Signals.Where(x => x.Key < time).ToList();
            foreach (var ok in query)
                tSignal.Signals.Remove(ok.Key);
        }



        protected virtual void DoDataChange(SNetDspTSignalSetSecF8 tSignal, SNetDspSignalSecF8EventArg newSignals)
        {
            var ea = new SNetDspSignalSetSecF8EventArg();
            ea.Sender = this;
            var time = newSignals.TSignal.Time.HasValue ? newSignals.TSignal.Time.Value : DateTime.Now;
            ea.Time = time;
            ea.TSignal = tSignal;
            ea.PrevTime = this.PrevTime;


            ea.NewTSignal.AddByKey(time, newSignals.TSignal.Signals);
            tSignal.AddByKey(time, newSignals.TSignal.Signals);
            this.OnDataChange(ea);

            this.PurgeSignal();

            this.PrevTime = time;

        }



        #region Event

        public event EventHandler<SNetDspSignalEventArg> evtDataChange;
        protected void OnDataChange(SNetDspSignalEventArg ea)
        {
            if (this.evtDataChange == null) return;
            this.evtDataChange(this, ea);
        }

        #endregion



        #region IDisposable

        protected override void DisposeSelf()
        {
            base.DisposeSelf();
        }

        #endregion

    }
}
