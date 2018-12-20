﻿using CToolkit.v0_1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.Dsp.Block
{
    public class SNetDspBlockBase : ISNetDspBlock, IDisposable
    {
        protected String _identifier = Guid.NewGuid().ToString();
        public string SNetDspIdentifier { get { return this._identifier; } set { this._identifier = value; } }

        ~SNetDspBlockBase() { this.Dispose(false); }

        #region IDisposable
        // Flag: Has Dispose already been called?
        bool disposed = false;

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
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
                this.DisposeManaged();
            }

            // Free any unmanaged objects here.
            //
            this.DisposeUnmanaged();
            this.DisposeSelf();
            disposed = true;
        }



      protected virtual  void DisposeManaged()
        {
        }
        protected virtual void DisposeUnmanaged()
        {

        }
        protected virtual void DisposeSelf()
        {
            CtkEventUtil.RemoveEventHandlersFrom((dlgt) => true, this);
        }
        #endregion
    }
}
