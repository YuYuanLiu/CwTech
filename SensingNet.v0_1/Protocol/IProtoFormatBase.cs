using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace SensingNet.v0_1.Protocol
{
    /// <summary>
    /// 處理Protocol Format相關功能
    /// </summary>
    public interface IProtoFormatBase
    {

        void FirstConnect(Stream stream);
        void ReceiveBytes(byte[] buffer, int offset, int length);
        bool IsReceiving();
        bool HasMessage();
        bool TryDequeueMsg(out object msg);

        //Protocol應具有能反譯通訊的能力
        void WriteMsg(Stream stream, String msg);
        void WriteMsg(Stream stream, byte[] buffer);
        void WriteMsgDataReq(Stream stream);
        void WriteMsgDataAck(Stream stream);





    }
}