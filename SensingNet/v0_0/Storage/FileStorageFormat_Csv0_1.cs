using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_0.Storage
{
    /// <summary>
    /// DateTime(yyyy/MM/dd HH:mm:ss+08),UTC timestamp, data1, data2, ...
    /// </summary>
    public class FileStorageFormat_Csv0_1: FileStorageFormat
    {
        public FileStorageFormat_Csv0_1()
        {
            this.FormatName = this.GetType().Name;
        }

    }
}
