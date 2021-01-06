using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;
using MathNet.Numerics;
using System.IO;
using System.Collections.Generic;
using System.Net;
using CToolkit.v1_1.Protocol;
using CToolkit.v1_1.Net;

namespace SensingNet.TestMy
{
    [TestClass]
    public class UtModbus
    {


        [TestMethod]
        public void TestMethod()
        {
            var nonStopTcpClient = new CtkTcpClient("127.0.0.1", 502);
            nonStopTcpClient.EhDataReceive += (ss, ee) =>
            {
                var ea = ee as CtkNonStopTcpStateEventArgs;
                var ctkBuffer = ea.TrxMessageBuffer;
                System.Diagnostics.Debug.WriteLine(ctkBuffer.Length);
            };
            nonStopTcpClient.NonStopRunAsyn();


            System.Threading.Thread.Sleep(1000);

            var msg = new CToolkit.v1_1.Modbus.CtkModbusMessage();
            msg.funcCode = CToolkit.v1_1.Modbus.CtkModbusMessage.fctReadHoldingRegister;
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
