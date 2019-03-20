using CToolkit.v1_0;
using CToolkit.v1_0.Timing;
using SensingNet.v0_1.TriggerDiagram.TimeSignal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.TriggerDiagram.Basic
{
    public class SNetTdNode : ISNetTdNode, IDisposable
    {
        public bool IsEnalbed = true;
        protected String _identifier = Guid.NewGuid().ToString();
        public string SNetDspIdentifier { get { return this._identifier; } set { this._identifier = value; } }
        public string SNetDspName { get; set; }
        public virtual void Close()
        {
            CtkEventUtil.RemoveEventHandlersFromOwningByFilter(this, (dlgt) => true);//移除自己的Event Delegate
        }


        

        #region IDisposable
        // Flag: Has Dispose already been called?
        protected bool disposed = false;

        // Public implementation of Dispose pattern callable by consumers.
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
                //
            }

            // Free any unmanaged objects here.
            //
            this.DisposeSelf();
            disposed = true;
        }



        protected virtual void DisposeSelf()
        {
            this.Close();
        }

        #endregion

    }
}
