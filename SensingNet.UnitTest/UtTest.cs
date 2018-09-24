using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;
using MathNet.Numerics;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Globalization;

namespace SensingNet.UnitTest
{
    [TestClass]
    public class UtTest
    {


        [TestMethod]
        public void TestMethod()
        {
            var format = "yyyy/MM/dd HH:mm:ss zzz";

            var now = DateTime.Now;
            var str = now.ToString(format);
            var normal = now.ToString();


            var dt = DateTime.ParseExact(str, format, CultureInfo.InvariantCulture);


        }


    }
}
