using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;
using CToolkit.Secs;
using System.Net;
using SensingNet.SecsMgr;
using CToolkit.Net;
using System.Text;
using System.Collections.Generic;

namespace SensingNet.MyTest.GenCfg
{
    [TestClass]
    public class UtGenDeviceCfg
    {
        [TestMethod]
        public void Test()
        {
            var list = new List<SignalMgr.SignalCfg>();
            list.Add(new SignalMgr.SignalCfg()
            {
                DeviceSvid = 0,
                CalibrateSysScale = 1.4305115598745083734100087529426e-6,
                CalibrateSysOffset = -12,
                CalibrateUserScale = 1,
                CalibrateUserOffset = 0,
                StorageDirectory = "signals/toolid/svid",
            });

            new SensingNet.SignalMgr.DeviceCfg()
            {
                RemoteIp = "192.168.123.201",
                RemotePort = 5000,
                DeviceName = "test201.vibartion",
                TxMode = Protocol.EnumProtocol.CwcCmd,
                IsActivelyTx = true,
                IsActivelyConnect = true,
                TxInterval = 2000,
                TimeoutResponse = 5000,
                SignalCfgList = list,
            }.SaveToXmlFile("simulate.device.config");


        }




    }
}
