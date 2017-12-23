using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;
using CToolkit.Secs;
using System.Net;
using SensingNet.SecsMgr;
using CToolkit.Net;
using System.Text;

namespace SensingNet.MyTest
{
    [TestClass]
    public class UtTest
    {
        [TestMethod]
        public void Test()
        {

            var a = CToolkit.CtkUtil.TryCatch(() =>
            {
                Console.WriteLine("AAA");
            });
            Console.WriteLine("B");
        }




    }
}
