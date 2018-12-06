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
    public interface ISNetProtoFormatBase
    {

        void ReceiveBytes(byte[] buffer, int offset, int length);
        bool IsReceiving();
        bool HasMessage();
        bool TryDequeueMsg(out object msg);







    }
}