using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;
using MathNet.Numerics;
using System.IO;
using System.Collections.Generic;

namespace SensingNet.MyTest
{
    [TestClass]
    public class UtSampleCaptureSignal
    {


        [TestMethod]
        public void TestMethod()
        {


            var deviceCfg = new v0_0.Signal.DeviceCfg();
            deviceCfg.RemoteIp = "192.168.123.201";
            deviceCfg.RemotePort = 10002;

            var signalCfg = new v0_0.Signal.SignalCfg();
            deviceCfg.SignalCfgList.Add(signalCfg);
            signalCfg.DeviceSvid = 0x00010000;


            var signalHandler = new v0_0.Signal.SignalHandler();
            signalHandler.config = deviceCfg;

            try
            {
                signalHandler.CfInit();
                signalHandler.CfLoad();
                while (true) { signalHandler.CfExec(); }
            }
            finally
            {
                signalHandler.CfUnLoad();
                signalHandler.CfFree();
            }


        }


    }
}
