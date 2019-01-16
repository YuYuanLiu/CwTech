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
using System.Numerics;
using Cudafy.Types;
using System.Runtime.InteropServices;

namespace SensingNet.MyTest
{




    [TestClass]
    public class UtTest
    {
        object prevtime = null;

        [TestMethod]
        public void TestMethod()
        {


            var ary1 = new Complex[3];
            var ary2 = new ComplexD[3];



            var cap1 = Marshal.SizeOf(new Complex());
            var cap2 = Marshal.SizeOf(new Complex());




            

        }



    }
}
