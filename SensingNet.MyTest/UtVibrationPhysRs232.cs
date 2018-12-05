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
using SensingNet.v0_1.Storage;
using SensingNet.v0_1.Protocol;
using CToolkit.v0_1.DigitalPort;

namespace SensingNet.MyTest
{
    [TestClass]
    public class UtVibrationPhysRs232
    {
        FileStorage fs = new FileStorage(@"signals/vibration");

        [TestMethod]
        public void TestMethod()
        {
            var deviceHdl = new v0_1.Device.SensorDeviceHandler();
            deviceHdl.Config = new v0_1.Device.SensorDeviceCfg()
            {
                RemoteIp = "192.168.123.201",
                RemotePort = 5000,
                SerialPortConfig = new CtkSerialPortCfg()
                {
                    PortName = "COM4",
                    BaudRate = 19200,
                },
                TxInterval = 0,
                //IsActivelyTx = true,
                TimeoutResponse = 5000,
                ProtoConnect = EnumProtoConnect.Rs232,
                ProtoFormat = EnumProtoFormat.SensingNetCmd,
                ProtoSession = EnumProtoSession.SensingNetCmd,
            };
            deviceHdl.Config.SignalCfgList.Add(new v0_1.Signal.SignalCfg()
            {
                Svid = 0,
            });
            deviceHdl.evtSignalCapture += (sender, ea) =>
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
