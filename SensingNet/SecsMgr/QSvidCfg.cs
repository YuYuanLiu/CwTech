using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.SecsMgr
{
    /// <summary>
    /// Query Svid
    /// </summary>
    public class QSvidCfg
    {
        public UInt32 QSvid;
        public EnumStatisticsMethod StatisticsMethod = EnumStatisticsMethod.Average;
        public int StatisticsSecond = 1;


        public String DeviceIp;//可為空值
        public int DevicePort;//可為零
        public string DeviceName;//若DeviceIp/Port不存在, 以Device Name區分
        public UInt32 DeviceSvid;

        public String StoragePath;


        public double DataTimeFailureSPEC_SEC;
        public double ExceptionDataTimeFailure;
        public double ExceptionDataNotFound;
        public double ExceptionDataFormatError;


        public EnumPassFilter PassFilter = EnumPassFilter.None;//使用FIR filter
        public int PassFilter_SampleRate = 512;
        public int PassFilter_CutoffHigh = 255;
        public int PassFilter_CutoffLow = 5;




        public void SaveToXmlFile(string fn) { CToolkit.CtkUtil.SaveToXmlFile(this, fn); }

    }

}
