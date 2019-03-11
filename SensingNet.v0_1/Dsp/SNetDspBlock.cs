using CToolkit.v0_1;
using CToolkit.v0_1.Timing;
using SensingNet.v0_1.Dsp.TimeSignal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.Dsp
{
    public class SNetDspBlock : SNetDspNode, ISNetDspNode
    {
        public Dictionary<String, SNetDspNode> DspNodes = new Dictionary<string, SNetDspNode>();
        public List<SNetDspWire> DspWires = new List<SNetDspWire>();


        public bool IsEnalbed = true;
        public int PurgeSeconds = 60;
        public Object PrevTime;//存放結構時:CtkTimeSecond, 仍可為null, 因此本身是物件形態
        ~SNetDspBlock() { this.Dispose(false); }



        public SNetDspNode AddNode(SNetDspNode node)
        {
            if (this.DspNodes.ContainsKey(node.SNetDspIdentifier)) throw new ArgumentException("Already exist identifier");
            this.DspNodes[node.SNetDspIdentifier] = node;
            return node;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="tSignal">來源資料, 集合會被修改</param>
        /// <param name="time"></param>
        /// <param name="newDatas"></param>
        protected virtual void DoDataChange(SNetDspTimeSignalSetSecond tSignal, CtkTimeSecond time, IEnumerable<double> newDatas)
        {
            var ea = new SNetDspTimeSignalSetSecondEventArg();
            ea.Time = time;
            ea.Sender = this;
            ea.TSignal = tSignal;
            ea.PrevTime = (CtkTimeSecond?)this.PrevTime;

            ea.NewTSignal.AddByKey(time, newDatas);
            tSignal.AddByKey(time, newDatas);
            this.OnDataChange(ea);

            this.PurgeSignal();

            this.PrevTime = time;

        }
        protected virtual void PurgeSignal()
        {
            throw new NotImplementedException();
        }
        protected virtual void PurgeSignalByCount(SNetDspTimeSignalSetSecond tSignal, int Count)
        {
            var query = tSignal.Signals.Take(tSignal.Signals.Count - Count).ToList();
            foreach (var ok in query)
                tSignal.Signals.Remove(ok.Key);
        }
        protected virtual void PurgeSignalByTime(SNetDspTimeSignalSetSecond tSignal, CtkTimeSecond time)
        {
            var now = DateTime.Now;
            var query = tSignal.Signals.Where(x => x.Key < time).ToList();
            foreach (var ok in query)
                tSignal.Signals.Remove(ok.Key);
        }
        protected virtual void PurgeSignalByTime(SNetDspTimeSignalSetMilliSecond tSignal, CtkTimeMilliSecond time)
        {
            var now = DateTime.Now;
            var query = tSignal.Signals.Where(x => x.Key < time).ToList();
            foreach (var ok in query)
                tSignal.Signals.Remove(ok.Key);
        }
        #region Event

        public event EventHandler<SNetDspTimeSignalEventArg> evtDataChange;
        protected void OnDataChange(SNetDspTimeSignalEventArg ea)
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
