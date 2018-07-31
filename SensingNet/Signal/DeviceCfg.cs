using SensingNet.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SensingNet.Signal
{

    [Serializable]
    public class DeviceCfg
    {
        public String LocalIp;
        public int LocalPort;
        public String RemoteIp = "192.168.123.101";
        public int RemotePort = 5000;

        public int DeviceId = 0;
        public String DeviceName = null;

        public EnumProtocol TxMode = EnumProtocol.Secs;
        public bool IsActivelyConnect = false;//Device是否為主動連線
        public bool IsActivelyTx = false;//Device是否會主動發訊息
        public int TxInterval = 0; // ms, 0=即時
        public int TimeoutResponse = 1000;

        public String ToolId = null;
        public String ToolName = null;

        public List<SignalCfg> SignalCfgList = new List<SignalCfg>();




        public void SaveToXmlFile(string fn) { CToolkit.CtkUtil.SaveToXmlFileT(this, fn); }
    }
}
