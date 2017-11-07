using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.SignalMgr
{
    public class SignalCfg
    {

        public String ToolId = null;
        public String ToolName = null;
        public UInt32 DeviceSvid = 0;
        public String SignalName;


        public double CalibrateSysScale = 1.0;
        public double CalibrateSysOffset = 0.0;

        public double CalibrateUserScale = 1.0;
        public double CalibrateUserOffset = 0.0;


        public String StorageDirectory = "Signals/toolid/svid";

    }
}
