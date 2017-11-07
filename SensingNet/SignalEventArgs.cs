using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet
{
    public class SignalEventArgs : EventArgs
    {
        public Int32 DeviceId;
        public String DeviceName;
        public String DeviceIp;
        public Int32 DevicePort;
        public String ToolId;
        public String ToolName;
        public DateTime RcvDateTime;

        //Device可以傳不同種類的訊號過來, 以此區分
        public UInt32 DeviceSvid;

        public List<double> Data = new List<double>();
        public List<double> calibrateData = new List<double>();

    }
}
