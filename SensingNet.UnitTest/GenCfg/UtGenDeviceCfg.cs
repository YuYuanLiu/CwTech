using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;
using CToolkit.Secs;
using System.Net;
using SensingNet.Secs;
using CToolkit.Net;
using System.Text;
using System.Collections.Generic;
using System.IO;

namespace SensingNet.MyTest.GenCfg
{
    [TestClass]
    public class UtGenDeviceCfg
    {

        String rootFolder = "../../../SensingNet/Config";

        [TestMethod]
        public void Test()
        {


            var dirInfo = new DirectoryInfo(Path.Combine(rootFolder));
            if (!dirInfo.Exists) dirInfo.Create();


        }



        void Vibration(DirectoryInfo dirInfo)
        {

            var list = new List<Signal.SignalCfg>();
            list.Add(new Signal.SignalCfg()
            {
                DeviceSvid = 0,
                CalibrateSysScale = 1.4305115598745083734100087529426e-6,
                CalibrateSysOffset = -12,
                CalibrateUserScale = 1,
                CalibrateUserOffset = 0,
                StorageDirectory = "signals/toolid/svid",
            });



            new SensingNet.Signal.DeviceCfg()
            {
                RemoteIp = "192.168.123.201",
                RemotePort = 5000,
                DeviceName = "test201.vibartion",
                TxMode = Protocol.EnumProtocol.SensingNetCmd,
                IsActivelyTx = true,
                IsActivelyConnect = true,
                TxInterval = 2000,
                TimeoutResponse = 5000,
                SignalCfgList = list,
            }.SaveToXmlFile(Path.Combine(dirInfo.FullName, "simulate.device.config"));

        }

        void Modbus(DirectoryInfo dirInfo)
        {

            var list = new List<Signal.SignalCfg>();
            list.Add(new Signal.SignalCfg()
            {
                DeviceSvid = 0,
                CalibrateSysScale = 1,
                CalibrateSysOffset = 0,
                CalibrateUserScale = 1,
                CalibrateUserOffset = 0,
                StorageDirectory = "signals/toolid/svid",
            });



            new SensingNet.Signal.DeviceCfg()
            {
                RemoteIp = "192.168.123.201",
                RemotePort = 5000,
                DeviceName = "test201.vibartion",
                TxMode = Protocol.EnumProtocol.SensingNetCmd,
                IsActivelyTx = true,
                IsActivelyConnect = true,
                TxInterval = 2000,
                TimeoutResponse = 5000,
                SignalCfgList = list,
            }.SaveToXmlFile(Path.Combine(dirInfo.FullName, "simulate.device.config"));

        }
    }
}
