﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;
using CToolkit.Secs;
using System.Net;
using SensingNet.Secs;
using CToolkit.Net;
using System.Text;

namespace SensingNet.MyTest
{
    [TestClass]
    public class UtSocket
    {
        [TestMethod]
        public void Test()
        {


            CToolkit.Net.EqTest_CtkTcpClient_Asyn.Test(
                "127.0.0.1",
                5001,
                delegate (EqTest_CtkTcpClient_Asyn obj)
                {
                    return 0;
                },
                delegate (EqTest_CtkTcpClient_Asyn obj, byte[] buffer, int length)
                {
                    var cmd = Encoding.UTF8.GetString(buffer, 0, length);
                    System.Diagnostics.Debug.WriteLine(cmd);

                    return 0;
                },
                delegate (EqTest_CtkTcpClient_Asyn obj)
                {
                    if (obj.client == null || !obj.client.Connected) return 0;
                    var stream = obj.client.GetStream();
                    var buff = Encoding.UTF8.GetBytes("Hello Server\r\n");
                    stream.Write(buff, 0, buff.Length);
                    return 0;
                }
            );

        }




    }
}
