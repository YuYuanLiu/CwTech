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
        public String LocalIp;
        public int LocalPort;
        public String RemoteIp = "192.168.123.101";
        public int RemotePort = 5000;

        public CtkSerialPortCfg SerialPortConfig = new CtkSerialPortCfg();

        public int DeviceId = 0;
        public String DeviceName = null;

        public SNetEnumProtoConnect ProtoConnect = SNetEnumProtoConnect.Tcp;
        public SNetEnumProtoFormat ProtoFormat = SNetEnumProtoFormat.Secs;
        public SNetEnumProtoSession ProtoSession = SNetEnumProtoSession.Secs;
        public SNetEnumSignalTran SignalTran = SNetEnumSignalTran.SensingNet;
        public bool IsActivelyConnect = false;//Device是否為主動連線
        public bool IsActivelyTx = false;//Device是否會主動發訊息
        public int TxInterval = 0; // ms, 0=即時
        public int TimeoutResponse = 1000;

        public List<SNetSignalCfg> SignalCfgList = new List<SNetSignalCfg>();




        public void SaveToXmlFile(string fn) { CtkUtil.SaveToXmlFileT(this, fn); }
    }
}
