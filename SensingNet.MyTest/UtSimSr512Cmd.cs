using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MathNet.Numerics;
using System.ComponentModel;
using CToolkit.Net;
using SensingNet.Signal;
using System.Collections.Generic;
using SensingNet.Protocol;

namespace SensingNet.MyTest
{
    [TestClass]
    public class UtSimSr512Cmd
    {
        [TestMethod]
        public void TestMethod1()
        {

            var waves = new List<double[]>();
            double[] finalWave = null;
            {
                var length = 2048;
                var samplingRate = 512;

                waves.Add(Generate.Sinusoidal(length, samplingRate, 16, 5));
                waves.Add(Generate.Sinusoidal(length, samplingRate, 32, 3));
                waves.Add(Generate.Sinusoidal(length, samplingRate, 64, 1));

                finalWave = new double[length];
                foreach (var w in waves)
                {
                    for (int idx = 0; idx < finalWave.Length; idx++)
                    {
                        finalWave[idx] += w[idx];
                    }
                }
            }



            var scfglist = new List<SignalCfg>();
            scfglist.Add(new SignalCfg()
            {
                DeviceSvid = 0,
                CalibrateSysScale = 1,
                CalibrateSysOffset = 0,
                CalibrateUserScale = 1,
                CalibrateUserOffset = 0,
                StorageDirectory = "signals/unittest",
            });


            var dcfg = new DeviceCfg()
            {
                RemoteIp = "127.0.0.1",
                RemotePort = 5000,
                TxMode = EnumProtocol.SensingNetCmd,
                IsActivelyConnect = false,
                IsActivelyTx = false,
                TxInterval = 0,
                TimeoutResponse = 2000,

                SignalCfgList = scfglist,
            };
            var di = new System.IO.DirectoryInfo("Config/DeviceConfigs/");
            di.Delete(true);
            dcfg.SaveToXmlFile("Config/DeviceConfigs/unittest.device.config");



            using (var signalmgr = new Signal.SignalMgrExecer())
            {

                signalmgr.evtSignalCapture += delegate (object sender, SignalEventArgs e)
                {

                };
                CToolkit.CtkUtil.RunWorkerAsyn(delegate (object sen, DoWorkEventArgs dwea)
                {
                    try
                    {
                        signalmgr.CfInit();
                        signalmgr.CfLoad();
                        signalmgr.CfRun();
                        signalmgr.CfUnload();
                        signalmgr.CfFree();
                    }
                    catch (Exception ex)
                    {
                        Assert.Fail(ex.Message);
                    }
                });



                var rcvWave = new List<double>();
                var sendIdx = 0;

                Test_CtkTcpListener_Asyn.Test(
                      "127.0.0.1",
                      5000,
                      delegate (Test_CtkTcpListener_Asyn obj)
                      {

                          return 0;
                      },
                      delegate (Test_CtkTcpListener_Asyn obj, byte[] buffer, int length)
                      {
                          if (!obj.acceptClient.Connected) return 0;
                          System.Threading.Thread.Sleep(1);
                          sendIdx = (sendIdx + 1) % finalWave.Length;
                          obj.WriteMsg("cmd -respData -svid 0 -data " + finalWave[sendIdx]);

                          return 0;
                      },
                      delegate (Test_CtkTcpListener_Asyn obj)
                      {
                          return 0;
                      }
                    );


                while (!signalmgr.isExec) { System.Threading.Thread.Sleep(1000); }
                while (signalmgr.isExec) { System.Threading.Thread.Sleep(1000); }

                signalmgr.isExec = false;



            }









        }
    }
}
