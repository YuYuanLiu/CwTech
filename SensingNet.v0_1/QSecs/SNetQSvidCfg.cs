using CToolkit.v1_0;
using CToolkit.v1_0.Numeric;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.QSecs
{
    /// <summary>
    /// Query Svid
    /// </summary>
    public class SNetQSvidCfg
    {
        public UInt64 QSvid;
        public SNetEnumStatisticsMethod StatisticsMethod = SNetEnumStatisticsMethod.Average;
        public int StatisticsSecond = 1;


        public String DeviceIp;//可為空值
        public int DevicePort;//可為零
        public string DeviceName;//若DeviceIp/Port不存在, 以Device Name區分
        public UInt64 DeviceSvid;

        public String StoragePath;



        public void SaveToXmlFile(string fn) { CtkUtil.SaveToXmlFileT(this, fn); }

    }

}
