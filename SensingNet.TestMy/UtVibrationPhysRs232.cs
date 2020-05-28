using Microsoft.VisualStudio.TestTools.UnitTesting;
using SensingNet.v0_2.DvcSensor;
using SensingNet.v0_2.Framework.Storage;
using SensingNet.v0_2.Protocol;
using SensingNet.v0_2.Storage;

namespace SensingNet.TestMy
{
    [TestClass]
    public class UtVibrationPhysRs232
    {
        SNetFileStorage fs = new SNetFileStorage(@"signals/vibration");

        [TestMethod]
        public void TestMethod()
        {
            var deviceHdl = new SNetDvcSensorHandler();
            deviceHdl.Config = new SNetDvcSensorCfg()
            {
                RemoteUri = "tcp://192.168.123.201:5000",
                TxInterval = 0,
                //IsActivelyTx = true,
                TimeoutResponse = 5000,
                ProtoConnect = SNetEnumProtoConnect.Rs232,
                ProtoFormat = SNetEnumProtoFormat.SNetCmd,
                ProtoSession = SNetEnumProtoSession.SNetCmd,
            };
            deviceHdl.Config.SignalCfgList.Add(new v0_2.SignalTrans.SNetSignalTransCfg()
            {
                Svid = 0,
            });
            deviceHdl.EhSignalCapture += (sender, ea) =>
            {
                fs.Write(ea);
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
