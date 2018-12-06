using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1
{
    [Serializable]
    public class SNetException : Exception
    {
        public SNetException(string msg) : base(msg) { }


    }
}
