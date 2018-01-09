using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.SignalMgr
{
    public class SignalMgrFileStorageEventArgs : EventArgs
    {
        public SignalHandler handler;
        public Storage.FileStorageEventArgs fileStorageEventArgs;

    }
}
