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
using CToolkit.v0_1;
using SensingNet.v0_1.Dsp.Block;

namespace SensingNet.MyTest
{




    [TestClass]
    public class UtTest
    {

        [TestMethod]
        public void TestMethod()
        {

            var fft = new SNetDspBlockFft();
            var collector = new SNetDspBlockSeqDataCollector();

            fft.Input = collector;


            CtkEventUtil.RemoveEventHandlersFromOwningByTarget(collector, fft);






        }



    }
}
