using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;
using CToolkit.Net;
using SensingNet.Alarm;

namespace SensingNet.MyTest
{
    [TestClass]
    public class UtAlarm
    {
        [TestMethod]
        public void TestMethod1()
        {



            using (var signalMgr = new Signal.SignalMgrExecer())
            using (var alarmMgr = new Alarm.AlarmMgrExecer())
            {
                signalMgr.evtSignalCapture += delegate (object sender, SignalEventArgs e)
                {
                    alarmMgr.DoAlarmCheck(e);
                };

                alarmMgr.evtAlarm += delegate (object sender, AlarmEventArgs e)
                {
                    System.Diagnostics.Debug.WriteLine("Alarm!!!");
                };


                CToolkit.CtkUtil.RunWorkerAsyn(delegate (object sender, DoWorkEventArgs e)
                {
                    alarmMgr.CfInit();
                    alarmMgr.CfLoad();
                    for (int idx = 0; idx < 10 && !signalMgr.isExec; idx++) { System.Threading.Thread.Sleep(1000); }
                    while (signalMgr.isExec) { System.Threading.Thread.Sleep(1000); }
                    alarmMgr.CfUnload();
                    alarmMgr.CfFree();
                });

                CToolkit.CtkUtil.RunWorkerAsyn(delegate (object sender, DoWorkEventArgs e)
                {
                    signalMgr.CfInit();
                    signalMgr.CfLoad();
                    signalMgr.CfRun();
                    signalMgr.CfUnload();
                    signalMgr.CfFree();
                });


                CToolkit.Net.EqTest_CtkTcpListener_Asyn.Test(
                    "127.0.0.1",
                     5000,
                     delegate (EqTest_CtkTcpListener_Asyn obj) { return 0; },
                     delegate (EqTest_CtkTcpListener_Asyn obj, byte[] buffer, int length)
                     {

                         var rnd = new Random(DateTime.Now.Second);
                         var val = rnd.NextDouble() - 0.5;
                         obj.WriteMsg(String.Format("cmd -respData -data {0}    \r\n", val));
                         return 0;
                     },
                     delegate (EqTest_CtkTcpListener_Asyn obj) { return 0; }
                );


                for (int idx = 0; idx < 10 && !signalMgr.isExec; idx++) { System.Threading.Thread.Sleep(1000); }
                while (signalMgr.isExec) { System.Threading.Thread.Sleep(1000); }
                signalMgr.isExec = false;



            }








        }





    }
}
