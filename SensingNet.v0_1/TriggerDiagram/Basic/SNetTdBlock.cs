using CToolkit.v1_0;
using CToolkit.v1_0.Timing;
using CToolkit.v1_0.TriggerDiagram;
using SensingNet.v0_1.TriggerDiagram.TimeSignal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.TriggerDiagram.Basic
{
    public class SNetTdBlock : SNetTdNode, ICtkTdBlock
    {
        public Dictionary<String, SNetTdNode> TdNodes = new Dictionary<string, SNetTdNode>();


        //存放結構時:CtkTimeSecond, 仍可為null, 因此本身是物件形態
        ~SNetTdBlock() { this.Dispose(false); }


        public T AddNode<T>() where T : SNetTdNode, new()
        {
            var node = new T();
            this.TdNodes[node.CtkTdIdentifier] = node;
            return node;
        }

        public T AddNode<T>(T node) where T : SNetTdNode
        {
            if (this.TdNodes.ContainsKey(node.CtkTdIdentifier)) throw new ArgumentException("Already exist identifier");
            this.TdNodes[node.CtkTdIdentifier] = node;
            return node;
        }


        public void RefreshNodeId()
        {
            var dspNodes = this.TdNodes.Values;
            lock (this.TdNodes)
            {
                this.TdNodes.Clear();
                foreach (var node in dspNodes)
                    this.TdNodes[node.CtkTdIdentifier] = node;
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
