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
    [TestClass]
    public class UtTest
    {


        [TestMethod]
        public void TestMethod()
        {
            var tasks = new List<Task>();

            var listener = new CToolkit.Net.CtkNonStopTcpListener("127.0.0.1", 5003);
            tasks.Add(Task.Run(() =>
            {
                listener.NonStopConnect();
            }));

            var client = new CToolkit.Net.CtkNonStopTcpClient("127.0.0.1", 5003);
            var flagClient = false;
            tasks.Add(Task.Run(() =>
            {
                client.ConnectIfNo();

                SpinWait.SpinUntil(() => client.IsRemoteConnected);

                SpinWait.SpinUntil(() => flagClient);

                client.Disconnect();


            }));

            var flag = false;
            SpinWait.SpinUntil(() => flag);



        }


    }
}
