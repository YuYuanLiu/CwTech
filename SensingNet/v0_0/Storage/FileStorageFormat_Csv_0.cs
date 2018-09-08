using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_0.Storage
{
    /// <summary>
    /// TC timestamp, data1, data2, ...
    /// </summary>

    public class FileStorageFormat_Csv_0:FileStorageFormat
    {
        
        public FileStorageFormat_Csv_0()
        {
            this.FormatName = this.GetType().Name;
        }

    }
}
