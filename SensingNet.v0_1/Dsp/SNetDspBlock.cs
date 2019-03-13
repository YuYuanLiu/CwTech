using CToolkit.v0_1;
using CToolkit.v0_1.Timing;
using SensingNet.v0_1.Dsp.TimeSignal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.Dsp
{
    public class SNetDspBlock : SNetDspNode, ISNetDspNode
    {
        public ConcurrentDictionary<String, SNetDspNode> DspNodes = new ConcurrentDictionary<string, SNetDspNode>();
        public List<SNetDspWire> DspWires = new List<SNetDspWire>();


        public bool IsEnalbed = true;
        public Object PrevTime;
        public int PurgeSeconds = 60;
        //存放結構時:CtkTimeSecond, 仍可為null, 因此本身是物件形態
        ~SNetDspBlock() { this.Dispose(false); }


        public T AddNode<T>() where T : SNetDspNode, new()
        {
            var node = new T();
            this.DspNodes[node.SNetDspIdentifier] = node;
            return node;
        }

        public T AddNode<T>(T node) where T : SNetDspNode
        {
            if (this.DspNodes.ContainsKey(node.SNetDspIdentifier)) throw new ArgumentException("Already exist identifier");
            this.DspNodes[node.SNetDspIdentifier] = node;
            return node;
        }
        public SNetDspWire<T> AddWire<T>( Delegate evt, EventHandler<T> destination)
            where T : EventArgs
        {
            var wire = new SNetDspWire<T>();
            //source += destination;

            

            return wire;
        }


        public void RefreshNodeId()
        {
            var dspNodes = this.DspNodes.Values;
            lock (this.DspNodes)
            {
                this.DspNodes.Clear();
                foreach (var node in dspNodes)
                    this.DspNodes[node.SNetDspIdentifier] = node;
            }
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
