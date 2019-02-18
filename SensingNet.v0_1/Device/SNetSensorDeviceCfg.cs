using CToolkit.v0_1;
using CToolkit.v0_1.DigitalPort;
using SensingNet.v0_1.Protocol;
using SensingNet.v0_1.Signal;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SensingNet.v0_1.Device
{

    [Serializable]
    public class SNetSensorDeviceCfg
    {
        public int DeviceId = 0;
        public String DeviceName = null;
        public bool IsActivelyConnect = false;
        //Device是否為主動連線
        public bool IsActivelyTx = false;

        public String LocalIp;
        public int LocalPort;
        public SNetEnumProtoConnect ProtoConnect = SNetEnumProtoConnect.Tcp;
        public SNetEnumProtoFormat ProtoFormat = SNetEnumProtoFormat.Secs;
        public SNetEnumProtoSession ProtoSession = SNetEnumProtoSession.Secs;
        public String RemoteIp = "192.168.123.101";
        public int RemotePort = 5000;
        public int IntervalOfNonStopConnect = 1000;

        public CtkSerialPortCfg SerialPortConfig = new CtkSerialPortCfg();
        public string Uri;
        public List<SNetSignalCfg> SignalCfgList = new List<SNetSignalCfg>();
        public SNetEnumSignalTran SignalTran = SNetEnumSignalTran.SNetCmd;
        public int TimeoutResponse = 1000;

        //Device是否會主動發訊息
        public int TxInterval = 0; // ms, 0=即時
        public void SaveToXmlFile(string fn) { CtkUtil.SaveToXmlFileT(this, fn); }
    }
}
