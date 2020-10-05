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
using CToolkit.v1_1.Numeric;
using System.Numerics;
using Cudafy.Types;
using System.Runtime.InteropServices;
using CToolkit.v1_1;
using System.Reflection;
using CToolkit.v1_1.Net;
using SensingNet.v0_2.TdSignalProc;

namespace SensingNet.TestMy
{

    [TestClass]
    public class UtTest
    {

        [TestMethod]
        public void TestMethod()
        {

            var tdnData = new SNetTdnSeqDataCollector();

            tdnData.TSignalSet[DateTime.Now] = new List<double>();

            var size = tdnData.GetMemorySizeJson();
            Console.WriteLine(size);




        }

  
    }
}
