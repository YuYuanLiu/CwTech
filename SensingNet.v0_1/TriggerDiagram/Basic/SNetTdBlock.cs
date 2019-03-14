using CToolkit.v0_1;
using CToolkit.v0_1.Timing;
using SensingNet.v0_1.TriggerDiagram.TimeSignal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.TriggerDiagram.Basic
{
    public class SNetTdBlock : SNetTdNode, ISNetTdBlock
    {
        public Dictionary<String, SNetTdNode> DspNodes = new Dictionary<string, SNetTdNode>();
        public List<SNetTdWire> DspWires = new List<SNetTdWire>();


        //存放結構時:CtkTimeSecond, 仍可為null, 因此本身是物件形態
        ~SNetTdBlock() { this.Dispose(false); }


        public T AddNode<T>() where T : SNetTdNode, new()
        {
            var node = new T();
            this.DspNodes[node.SNetDspIdentifier] = node;
            return node;
        }

        public T AddNode<T>(T node) where T : SNetTdNode
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
