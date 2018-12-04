using CToolkit.Secs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.Protocol
{
    public class ProtoSessionSecs : IProtoSessionBase
    {

        public bool ProcessSession(IProtoConnectBase protoConn, object msg)
        {
            var secsMsg = msg as HsmsMessage;
            if (secsMsg == null) throw new ArgumentException("不正確的msg型態");

            switch (secsMsg.header.SType)
            {
                case 1:
                    protoConn.WriteBytes(HsmsMessage.CtrlMsg_SelectRsp(0).ToBytes());
                    return true;
                case 2:
                    return true;
                case 5:
                    protoConn.WriteBytes(HsmsMessage.CtrlMsg_LinktestRsp().ToBytes());
                    return true;
                case 6:
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="protoConn">並非所有通訊都是繼續自Stream, 因此請實作IProtoConnectBase</param>
        public void FirstConnect(IProtoConnectBase protoConn)
        {
            var txMsg = HsmsMessage.CtrlMsg_SelectReq();
            protoConn.WriteBytes(txMsg.ToBytes());
            txMsg = HsmsMessage.CtrlMsg_LinktestReq();
            protoConn.WriteBytes(txMsg.ToBytes());
        }



    }
}
