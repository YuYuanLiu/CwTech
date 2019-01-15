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
using CToolkit.v0_1.Numeric;

namespace SensingNet.MyTest
{




    [TestClass]
    public class UtTest
    {
        object prevtime = null;

        [TestMethod]
        public void TestMethod()
        {

            var pf = new CtkPassFilterStruct();
            pf.SampleRate = 1024;

            var pf2 = (CtkPassFilterStruct)this.prevtime;
            Change(pf);
            pf2 = (CtkPassFilterStruct)this.prevtime;

            Console.Write(pf.SampleRate);
        }




        void Change(object obj)
        {
            var pf = (CtkPassFilterStruct)obj;
            pf.SampleRate = 512;
            prevtime = pf;
        }

    }
}
