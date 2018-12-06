using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.Signal
{
    public class SNetSignalCfg
    {

        public UInt32 Svid = 0;
        public String Name;


        public double CalibrateSysScale = 1.0;
        public double CalibrateSysOffset = 0.0;

        public double CalibrateUserScale = 1.0;
        public double CalibrateUserOffset = 0.0;


        //public String StorageDirectory = "Signals/toolid/svid";
        //public long PurgeTimestamp = 180 * 24 * 60 * 60;//預設Purge Rule 

    }
}
