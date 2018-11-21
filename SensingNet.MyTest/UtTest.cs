using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;
using MathNet.Numerics;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Globalization;
using System.Text;

namespace SensingNet.MyTest
{
    [TestClass]
    public class UtTest
    {


        [TestMethod]
        public void TestMethod()
        {

            var dict = new Dictionary<string, object>();
            dict["A"] = 1;

            dict["A"] = null;

        }


    }
}
