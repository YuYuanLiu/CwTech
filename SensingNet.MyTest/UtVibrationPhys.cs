﻿using System;
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

namespace SensingNet.MyTest
{
    [TestClass]
    public class UtVibrationPhys
    {
        FileStorage fs = new FileStorage(@"signals/vibration");

        [TestMethod]
        public void TestMethod()
        {
            var deviceHdl = new v0_1.Device.DeviceSensorHandler();
            deviceHdl.Config = new v0_1.Device.DeviceSensorCfg()
            {
                RemoteIp = "192.168.123.201",
                RemotePort = 5000,
                IsActivelyTx = true,
                TxInterval = 0,
                TimeoutResponse = 5000,
                ProtoFormat = v0_1.Device.EnumDeviceProtoFormat.SensingNetCmd,
                IsActivelyConnect = false,
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
