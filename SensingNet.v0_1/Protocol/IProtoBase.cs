using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace SensingNet.v0_1.Protocol
{
    public interface IProtoBase
    {

        void FirstConnect(Stream stream);
        void ReceiveBytes(byte[] buffer, int offset, int length);
        bool IsReceiving();
        bool hasMessage();
        bool AnalysisData(Stream stream);
        void WriteMsg(Stream stream, String msg);
        void WriteMsg(Stream stream, byte[] buffer);
        void WriteMsg_Tx(Stream stream);
        void WriteMsg_TxDataReq(Stream stream);
        void WriteMsg_TxDataAck(Stream stream);



        #region Event

        public event EventHandler<EventArgs> evtDataTrigger;
        public void OnDataTrigger(EventArgs ea)
        {
            if (this.evtDataTrigger == null)
                return;
            this.evtDataTrigger(this, ea);
        }


        #endregion

    }
}
