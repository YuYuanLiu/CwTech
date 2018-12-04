using CToolkit.Secs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.Protocol
{
    public class ProtoSessionSensingNetCmd : IProtoSessionBase
    {

        public bool ProcessSession(IProtoConnectBase protoConn, object msg)
        {
            return false;
        }

        public void FirstConnect(IProtoConnectBase protoConn)
        {
        }




    }
}
