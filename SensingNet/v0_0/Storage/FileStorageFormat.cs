using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SensingNet.v0_0.Storage
{
    public class FileStorageFormat
    {
        public String FormatName = typeof(FileStorageFormat).Name;
        public String Remark;



        public virtual void WriteValues(StreamWriter sw, DateTime utc, IEnumerable<double> values)
        {
            throw new NotImplementedException();
        }


        public virtual void ReadStream(StreamReader sr, SignalCollector collector)
        {
            throw new NotImplementedException();
        }
    }
}
