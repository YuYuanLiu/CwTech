using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_0
{
    [Serializable]
    public class SensingNetException : Exception
    {
        public SensingNetException(string msg) : base(msg) { }


    }
}
