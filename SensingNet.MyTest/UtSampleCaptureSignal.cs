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


            var deviceCfg = new Signal.DeviceCfg();
            deviceCfg.RemoteIp = "192.168.123.201";
            deviceCfg.RemotePort = 10002;

            var signalCfg = new Signal.SignalCfg();
            deviceCfg.SignalCfgList.Add(signalCfg);
            signalCfg.DeviceSvid = 0x00010000;


            var signalHandler = new Signal.SignalHandler();
            signalHandler.config = deviceCfg;

            try
            {
                signalHandler.CfInit();
                signalHandler.CfLoad();
                while (true) { signalHandler.CfExec(); }
            }
            finally
            {
                signalHandler.CfUnload();
                signalHandler.CfFree();
            }


        }


    }
}
