using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;
using CToolkit.Secs;
using System.Net;
using SensingNet.SecsMgr;
using CToolkit.Net;
using System.Text;

namespace SensingNet.MyTest.GenCfg
{
    [TestClass]
    public class UtGenAlarmCfg
    {
        [TestMethod]
        public void Test()
        {
            new SensingNet.AlarmMgr.AlarmCfg()
            {
                DeviceName = "simulate",
                DeviceSvid = 0,
                Max = 0.5,
                Min = -0.5,

                PassFilter = EnumPassFilter.BandPass,
                PassFilter_SampleRate = 512,
                PassFilter_CutoffHigh = 250,
                PassFilter_CutoffLow = 5,
                AlarmIntervalSec = 60

            }.SaveToXmlFile("simulate.alarm.config");


        }




    }
}
