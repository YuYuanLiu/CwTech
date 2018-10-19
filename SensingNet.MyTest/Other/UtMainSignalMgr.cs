using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;
using CToolkit.Net;

namespace SensingNet.MyTest
{
    [TestClass]
    public class UtMainSignalMgr
    {
        [TestMethod]
        public void TestMethod1()
        {



            using (var signalMgr = new v0_0.Signal.SignalMgrExecer())
            {

                CToolkit.Logging.LoggerMapper.Singleton.Get().evtLogWrite += UtMainSignalMgr_evtLogWrite;


                CToolkit.CtkUtil.RunWorkerAsyn(delegate (object sender, DoWorkEventArgs e)
                {
                    signalMgr.CfInit();
                    signalMgr.CfLoad();
                    signalMgr.CfRun();
                    signalMgr.CfUnLoad();
                    signalMgr.CfFree();
                });






                while (!signalMgr.isExec) { }

                var flag = true;
                while (flag)
                {
                    System.Threading.Thread.Sleep(1000);
                }




            }








        }

        private void UtMainSignalMgr_evtLogWrite(object sender, CToolkit.Logging.LoggerEventArgs e)
        {

            System.Diagnostics.Debug.WriteLine(e.message);
        }
    }
}
