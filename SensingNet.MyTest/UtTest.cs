using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;
using MathNet.Numerics;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace SensingNet.MyTest
{

    class EventClass
    {
        public event EventHandler evtTest;
        public void OnTest()
        {
            if (this.evtTest == null) return;
            this.evtTest(this, null);

        }
    }


    class UseClass : IDisposable
    {

        ~UseClass() { this.Dispose(false); }

        public string name;
        public void Use(Object sender, EventArgs ea)
        {
            System.Diagnostics.Debug.WriteLine("Use " + this.name);
        }
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



        void DisposeManaged()
        {
        }
        void DisposeUnmanaged()
        {

        }
        void DisposeSelf()
        {

        }
        #endregion

    }


    [TestClass]
    public class UtTest
    {
        [TestMethod]
        public void TestMethod()
        {
            var useA = new UseClass() { name = "A" };
              var evtCls = new EventClass();
            evtCls.evtTest += useA.Use;

            TestUse(evtCls);

            evtCls.OnTest();





        }


        void TestUse(EventClass evtCls)
        {
            var useB = new UseClass() { name = "B" };
            evtCls.evtTest += useB.Use;
            using (useB) { }


            evtCls.evtTest -= useB.Use;

            GC.Collect();
            GC.WaitForPendingFinalizers();


        }


    }
}
