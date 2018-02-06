using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;
using CToolkit.Secs;
using System.Net;
using SensingNet.Secs;
using CToolkit.Net;
using System.Text;
using System.Globalization;

namespace SensingNet.MyTest
{
    [TestClass]
    public class UtTest
    {
        [TestMethod]
        public void Test()
        {


            var dt = DateTime.ParseExact("2018/01/14","yyyy/MM/dd", CultureInfo.InvariantCulture);
           var dd=  dt.ToLocalTime();
            var dd2 = dt.ToUniversalTime();
            var dd3 = dd2.ToUniversalTime();
            Console.WriteLine("B");
        }




    }
}
