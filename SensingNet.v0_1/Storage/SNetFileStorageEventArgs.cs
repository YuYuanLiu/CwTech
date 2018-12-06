using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.Storage
{
    public class SNetFileStorageEventArgs : EventArgs
    {
        public string PrevFilePath;
        public string CurrFilePath;

    }
}
