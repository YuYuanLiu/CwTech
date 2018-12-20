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
using CToolkit.v0_1.Net;
using System.Threading;
using SensingNet.v0_1.Protocol;
using SensingNet.v0_1.Device;

namespace SensingNet.MyTest.UnitTest
{
    [TestClass]
    public class UtCommunicationVibration
    {
        SNetFileStorage fs = new SNetFileStorage(@"signals/vibration");

        [TestMethod]
        public void TestMethod()
        {

            //設定旗標
            var startDt = DateTime.Now;
            var isFinishDevice = false;




            var deviceHdl = new SNetSensorDeviceHandler();
            deviceHdl.Config = new SNetSensorDeviceCfg()
            {
                RemoteIp = "127.0.0.1",
                RemotePort = 5003,
                IsActivelyTx = true,
                TxInterval = 0,
                TimeoutResponse = 5000,
                ProtoFormat = SNetEnumProtoFormat.SensingNetCmd,
                IsActivelyConnect = false,
            };
            deviceHdl.Config.SignalCfgList.Add(new v0_1.Signal.SNetSignalCfg()
            {
                Svid = 0,
            });
            deviceHdl.evtSignalCapture += (sender, ea) =>
            {
                fs.Write(ea);
            };


            Task.Run(() =>
            {
                using (deviceHdl)
                {
                    deviceHdl.CfInit();
                    deviceHdl.CfLoad();
                    deviceHdl.CfRun();
                    deviceHdl.CfUnLoad();
                    deviceHdl.CfFree();
                }
                isFinishDevice = true;
            });





            var deviceListener = new CtkNonStopTcpListener("127.0.0.1", 5003);
            var seqval = 0.000000001;
            deviceListener.evtDataReceive += (sender, ea) =>
            {
                var state = ea as CtkNonStopTcpStateEventArgs;
                var rnd = new Random((int)DateTime.Now.Ticks);

                //System.Threading.Thread.Sleep(1);

                //記得加換行, cmd 結構以換行為分界
                state.WriteMsg(string.Format("cmd -respData -svid 0 -data {0} {1}\n"
                    , seqval += 0.000000001
                    , rnd.NextDouble()
                    ));
            };
            deviceListener.NonStopConnectAsyn();



            SpinWait.SpinUntil(() => (DateTime.Now - startDt).TotalMinutes >= 1);
            deviceHdl.CfIsRunning = false;
            SpinWait.SpinUntil(() => isFinishDevice);



        }


    }
}
