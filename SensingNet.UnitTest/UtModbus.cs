using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;
using MathNet.Numerics;
using System.IO;
using System.Collections.Generic;
using System.Net;

namespace SensingNet.MyTest
{
    [TestClass]
    public class UtModbus
    {


        [TestMethod]
        public void TestMethod()
        {
            var remoteEp = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 502);
            var nonStopTcpClient = new CToolkit.Net.CtkNonStopTcpClient(null, remoteEp);
            nonStopTcpClient.evtDataReceive += delegate (object sender, CToolkit.Net.CtkNonStopTcpStateEventArgs e)
            {
                System.Diagnostics.Debug.WriteLine(e.length);
            };
            nonStopTcpClient.NonStopConnect();


            System.Threading.Thread.Sleep(1000);

            var msg = new CToolkit.Modbus.ModbusMessage();
            msg.funcCode = CToolkit.Modbus.ModbusMessage.fctReadHoldingRegister;
            msg.unitId = 1;
            msg.readLength = 32;
            var buffer = msg.ToRequestBytes();


            nonStopTcpClient.WriteMsg(buffer, buffer.Length);


            while (true)
            {
                System.Threading.Thread.Sleep(1000);
            }


        }


    }
}
