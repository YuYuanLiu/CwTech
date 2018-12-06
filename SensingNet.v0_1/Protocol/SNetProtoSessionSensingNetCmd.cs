using CToolkit.Secs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.Protocol
{
    public class SNetProtoSessionSensingNetCmd : ISNetProtoSessionBase
    {

        public bool ProcessSession(ISNetProtoConnectBase protoConn, object msg)
        {
            return false;
        }

        public void FirstConnect(ISNetProtoConnectBase protoConn)
        {
        }




    }
}
