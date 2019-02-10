using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;
using MathNet.Numerics;
using System.IO;
using System.Collections.Generic;
using System.Net;
using CToolkit.v0_1.Protocol;

namespace SensingNet.MyTest
{
    [TestClass]
    public class UtModbus
    {


        [TestMethod]
        public void TestMethod()
        {
            var remoteEp = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 502);
            var nonStopTcpClient = new CToolkit.v0_1.Net.CtkNonStopTcpClient(remoteEp);
            nonStopTcpClient.evtDataReceive += (ss, ee) =>
            {
                var ctkBuffer = ee.TrxMessageBuffer;
                System.Diagnostics.Debug.WriteLine(ctkBuffer.Length);
            };
            nonStopTcpClient.NonStopConnectAsyn();


            System.Threading.Thread.Sleep(1000);

            var msg = new CToolkit.v0_1.Modbus.CtkModbusMessage();
            msg.funcCode = CToolkit.v0_1.Modbus.CtkModbusMessage.fctReadHoldingRegister;
            msg.unitId = 1;
            msg.readLength = 32;
            var buffer = msg.ToRequestBytes();


            nonStopTcpClient.WriteMsg(buffer);


            while (true)
            {
                System.Threading.Thread.Sleep(1000);
            }


        }


    }
}
