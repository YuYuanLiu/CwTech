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
using System.Threading;
using SensingNet.v0_2.Framework;

namespace SensingNet.TestMy
{
    [TestClass]
    public class UtSample
    {


        [TestMethod]
        public void TestMethod()
        {

            var sensorDeviceMgr = new SNetSensorDeviceMgr();
            var qsecsMgr = new SNetQSecsMgr();
            var dspMgr = new SNetDspMgr();
            var alarmMgr = new SNetAlarmMgr();


            sensorDeviceMgr.EhSignalCapture += (sender, ea) =>
            {
                //TODO: dspMgr
            };

            qsecsMgr.EhReceiveData += (sender, ea) =>
            {
                //TODO:
            };




        }



    }
}
