using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensingNet.v0_1.Signal
{
    public class SNetSignalEventArgs : EventArgs
    {
        public object Sender;
        public string RemoteIp;
        public int RemotePort;
        public string DeviceName;

        public UInt64? Svid;
        public List<double> Data = new List<double>();
        public List<double> CalibrateData = new List<double>();
        public DateTime RcvDateTime;
        public SNetSignalCfg SignalConfig;

    }
}
