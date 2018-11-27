using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;
using MathNet.Numerics;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Globalization;
using System.Text;

namespace SensingNet.MyTest.UnitTest
{
    [TestClass]
    public class UtVibration
    {


        [TestMethod]
        public void TestMethod()
        {
            var deviceHdl = new v0_1.Device.DeviceHandler();
            deviceHdl.config = new v0_1.Device.DeviceCfg()
            {
                RemoteIp = "192.168.123.201",
                RemotePort = 5000,
                IsActivelyTx = true,
                TxInterval = 0,
                TimeoutResponse = 5000,
                TxMode = v0_1.Device.EnumDeviceProtocol.SensingNetCmd,
                IsActivelyConnect = false,
            };
            deviceHdl.config.SignalCfgList.Add(new v0_1.Signal.SignalCfg()
            {
                DeviceSvid = 0,
            });

            deviceHdl.evtSignalCapture += DeviceHdl_evtSignalCapture;


            deviceHdl.CfInit();
            deviceHdl.CfLoad();
            deviceHdl.CfExec();
            deviceHdl.CfUnLoad();
            deviceHdl.CfFree();


            while (true)
            {



                System.Threading.Thread.Sleep(100);
            }


        }

        private void DeviceHdl_evtSignalCapture(object sender, v0_1.Signal.SignalEventArgs e)
        {
            foreach (var val in e.CalibrateData)
                System.Diagnostics.Debug.Write(val + ",");
            System.Diagnostics.Debug.WriteLine("");


        }
    }
}
