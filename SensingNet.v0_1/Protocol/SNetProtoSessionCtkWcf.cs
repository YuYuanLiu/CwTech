using CToolkit.v1_0.Secs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.Protocol
{
    public class SNetProtoSessionCtkWcf : ISNetProtoSessionBase
    {

        public bool ProcessSession(ISNetProtoConnectBase protoConn, object msg)
        {
          

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="protoConn">並非所有通訊都是繼續自Stream, 因此請實作IProtoConnectBase</param>
        public void FirstConnect(ISNetProtoConnectBase protoConn)
        {
           
        }



    }
}
