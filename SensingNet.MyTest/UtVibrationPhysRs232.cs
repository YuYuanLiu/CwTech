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
using SensingNet.v0_2.Storage;
using SensingNet.v0_2.Protocol;
using CToolkit.v1_0.DigitalPort;
using SensingNet.v0_2.Device;

namespace SensingNet.MyTest
{
    [TestClass]
    public class UtVibrationPhysRs232
    {
        SNetFileStorage fs = new SNetFileStorage(@"signals/vibration");

        [TestMethod]
        public void TestMethod()
        {
            var deviceHdl = new SNetDvcSensorHandler();
            deviceHdl.Config = new SNetDvcSensorCfg()
            {
                RemoteUri = "tcp://192.168.123.201:5000",
                SerialPortConfig = new CtkSerialPortCfg()
                {
                    PortName = "COM4",
                    BaudRate = 19200,
                },
                TxInterval = 0,
                //IsActivelyTx = true,
                TimeoutResponse = 5000,
                ProtoConnect = SNetEnumProtoConnect.Rs232,
                ProtoFormat = SNetEnumProtoFormat.SNetCmd,
                ProtoSession = SNetEnumProtoSession.SNetCmd,
            };
            deviceHdl.Config.SignalCfgList.Add(new v0_2.Signal.SNetSignalCfg()
            {
                Svid = 0,
            });
            deviceHdl.EhSignalCapture += (sender, ea) =>
            {
                fs.Write(ea);
            };





            using (deviceHdl)
            {
                deviceHdl.CfInit();
                deviceHdl.CfLoad();
                deviceHdl.CfRun();
                deviceHdl.CfUnLoad();
                deviceHdl.CfFree();
                System.Threading.Thread.Sleep(100);
            }

        }


    }
}
