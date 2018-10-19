using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SensingNet.v0_0.Storage
{
    /// <summary>
    /// TC timestamp, data1, data2, ...
    /// </summary>
    [Obsolete("Please use FileStorageFormat_Csv0_1")]
    public class FileStorageFormat_Csv0_0 : FileStorageFormat_Csv_0
    {

        public FileStorageFormat_Csv0_0()
        {
            this.FormatName = this.GetType().Name;
        }


    }
}
