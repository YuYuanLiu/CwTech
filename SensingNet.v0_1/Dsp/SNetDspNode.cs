using CToolkit.v0_1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.Dsp
{
    public class SNetDspNode : ISNetDspNode, IDisposable
    {
        protected String _identifier = Guid.NewGuid().ToString();
        public string SNetDspIdentifier { get { return this._identifier; } set { this._identifier = value; } }
        public string SNetDspName { get; set; }



        public virtual void Close()
        {
            CtkEventUtil.RemoveEventHandlersFromOwningByFilter(this, (dlgt) => true);
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
