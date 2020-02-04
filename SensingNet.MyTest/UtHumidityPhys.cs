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
using SensingNet.v0_2.Signal;
using SensingNet.v0_2.Device;

namespace SensingNet.MyTest
{
    [TestClass]
    public class UtHumidityPhys
    {
        SNetFileStorage fs = new SNetFileStorage(@"signals/humidity");

        [TestMethod]
        public void TestMethod()
        {
            var deviceHdl = new SNetDvcSensorHandler();
            deviceHdl.Config = new SNetDvcSensorCfg()
            {
                RemoteUri = "tcp://192.168.123.201:5000",
                TxInterval = 0,
                TimeoutResponse = 5000,
                ProtoConnect = SNetEnumProtoConnect.Tcp,
                ProtoFormat = SNetEnumProtoFormat.SNetCmd,
                ProtoSession = SNetEnumProtoSession.SNetCmd,
                SignalTran = SNetEnumSignalTran.SNetCmd,
            };
            deviceHdl.Config.SignalCfgList.Add(new v0_2.Signal.SNetSignalCfg()
            {
                Svid = 0x00010000,
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
