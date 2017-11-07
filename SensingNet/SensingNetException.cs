using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet
{
    [Serializable]
    public class SensingNetException : Exception
    {
        public SensingNetException(string msg) : base(msg) { }
    }
}
