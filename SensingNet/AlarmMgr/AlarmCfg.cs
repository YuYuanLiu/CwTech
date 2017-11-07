using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.AlarmMgr
{

    public class AlarmCfg
    {

        public String DeviceIp;//可為空值
        public int DevicePort;//可為零
        public String DeviceName;//當DeviceIp/Port不存在時, 參考DeviceName
        public UInt32 DeviceSvid;

        public String ToolId;//盡量別用, 僅參考
        public String ToolName;//盡量別用, 僅參考

        public double Max;
        public double Min;

        public EnumPassFilter PassFilter = EnumPassFilter.None;//使用FIR filter
        public int PassFilter_SampleRate = 512;
        public int PassFilter_CutoffHigh = 255;
        public int PassFilter_CutoffLow = 5;

        public String Mail;
        public String PhoneNo;
        public long AlarmIntervalSec = 60 * 10;//異常通知的間隔秒數



        public void SaveToXmlFile(string fn) { CToolkit.CtkUtil.SaveToXmlFile(this, fn); }

    }

}
