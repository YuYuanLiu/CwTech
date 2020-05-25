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
using SensingNet.v0_2.Protocol;
using SensingNet.v0_2.SignalTrans;
using SensingNet.v0_2.DvcSensor;
using CToolkit.v1_1;
using SensingNet.v0_2.Framework.Storage;

namespace SensingNet.TestMy
{
    [TestClass]
    public class UtHumidityPhys
    {
        SNetFileStorage fs = new SNetFileStorage(@"signals/humidity");

        [TestMethod]
        public void TestMethod()
        {
            CtkLog.RegisterEveryLogWrite((ss, ee) =>
            {
                System.Diagnostics.Debug.WriteLine(ee.Message);
            });

            var deviceHdl = new SNetDvcSensorHandler();
            deviceHdl.Config = new SNetDvcSensorCfg()
            {
                RemoteUri = "tcp://192.168.123.201:5000",
                TxInterval = 0,
                TimeoutResponse = 5000,
                ProtoConnect = SNetEnumProtoConnect.Tcp,
                ProtoFormat = SNetEnumProtoFormat.SNetCmd,
                ProtoSession = SNetEnumProtoSession.SNetCmd,
                SignalTran = SNetEnumSignalTrans.SNetCmd,
            };
            for (var idx = 0; idx < 8; idx++)
            {
                deviceHdl.Config.SignalCfgList.Add(new v0_2.SignalTrans.SNetSignalTransCfg()
                {
                    Svid = 0x00010000 + 0x0100 * (ulong)idx,
                    //Svid = 0x00000000,
                });
            }
            for (var idx = 0; idx < 8; idx++)
            {
                deviceHdl.Config.SignalCfgList.Add(new v0_2.SignalTrans.SNetSignalTransCfg()
                {
                    Svid = 0x00020000 + 0x0100 * (ulong)idx,
                    //Svid = 0x00000000,
                });
            }

            deviceHdl.EhSignalCapture += (sender, ea) =>
            {
                fs.Write(ea);
                //if (ea.Data.Count > 0)
                    //System.Diagnostics.Debug.WriteLine("{0}={1}", ea.Svid, ea.Data[0]);
            };





            using (deviceHdl)
            {
                deviceHdl.CfInit();
                deviceHdl.CfLoad();
                deviceHdl.CfRunLoop();
                deviceHdl.CfUnLoad();
                deviceHdl.CfFree();
                System.Threading.Thread.Sleep(100);
            }

        }


    }
}
