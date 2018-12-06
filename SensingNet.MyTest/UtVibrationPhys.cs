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
using SensingNet.v0_1.Signal;

namespace SensingNet.MyTest
{
    [TestClass]
    public class UtVibrationPhys
    {
        SNetFileStorage fs = new SNetFileStorage(@"signals/vibration");

        [TestMethod]
        public void TestMethod()
        {
            var deviceHdl = new v0_1.Device.SNetSensorDeviceHandler();
            deviceHdl.Config = new v0_1.Device.SNetSensorDeviceCfg()
            {
                RemoteIp = "192.168.123.201",
                RemotePort = 5000,
                TxInterval = 0,
                TimeoutResponse = 5000,
                ProtoConnect = SNetEnumProtoConnect.Tcp,
                ProtoFormat = SNetEnumProtoFormat.SensingNetCmd,
                ProtoSession = SNetEnumProtoSession.SensingNetCmd,
                SignalTran = SNetEnumSignalTran.SensingNet,
            };
            deviceHdl.Config.SignalCfgList.Add(new v0_1.Signal.SNetSignalCfg()
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
