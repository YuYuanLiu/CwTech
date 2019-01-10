using CToolkit.v0_1.Secs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.Protocol
{
    public class SNetProtoSessionSecs : ISNetProtoSessionBase
    {

        public bool ProcessSession(ISNetProtoConnectBase protoConn, object msg)
        {
            var secsMsg = msg as CtkHsmsMessage;
            if (secsMsg == null) throw new ArgumentException("不正確的msg型態");

            switch (secsMsg.header.SType)
            {
                case 1:
                    protoConn.WriteBytes(CtkHsmsMessage.CtrlMsg_SelectRsp(0).ToBytes());
                    return true;
                case 2:
                    return true;
                case 5:
                    protoConn.WriteBytes(CtkHsmsMessage.CtrlMsg_LinktestRsp().ToBytes());
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
        public void FirstConnect(ISNetProtoConnectBase protoConn)
        {
            var txMsg = CtkHsmsMessage.CtrlMsg_SelectReq();
            protoConn.WriteBytes(txMsg.ToBytes());
            txMsg = CtkHsmsMessage.CtrlMsg_LinktestReq();
            protoConn.WriteBytes(txMsg.ToBytes());
        }



    }
}
