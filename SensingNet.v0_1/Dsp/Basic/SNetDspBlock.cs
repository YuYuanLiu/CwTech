using CToolkit.v0_1;
using CToolkit.v0_1.Timing;
using SensingNet.v0_1.Dsp.TimeSignal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.Dsp.Basic
{
    public class SNetDspBlock : SNetDspNode, ISNetDspNode
    {
        public ConcurrentDictionary<String, SNetDspNode> DspNodes = new ConcurrentDictionary<string, SNetDspNode>();
        public List<SNetDspWire> DspWires = new List<SNetDspWire>();


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
   


        #region IDisposable

        protected override void DisposeSelf()
        {
            base.DisposeSelf();
        }


        #endregion
    }
}
