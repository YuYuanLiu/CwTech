using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace SensingNet.SignalMgr
{
    public abstract class ProtoEthernetBase
    {
        public DeviceCfg dConfig;

        public virtual void FirstConnect(NetworkStream stream)
        {

        }

        public virtual void ReceiveBytes(byte[] buffer, int offset, int length)
        {
            throw new NotImplementedException();
        }



        /// <summary>
        /// 
        /// </summary>
        /// <returns>false: 資料接收中 ; true: 沒有在收資料</returns>
        public virtual bool IsReceiving() { throw new NotImplementedException(); }
        public virtual bool hasMessage() { throw new NotImplementedException(); }

        public virtual bool AnalysisData(NetworkStream stream)
        {
            throw new NotImplementedException();
        }

        public virtual void WriteMsg(NetworkStream stream, String msg)
        {
            if (!stream.CanWrite)
                return;
            var buffer = Encoding.UTF8.GetBytes(msg);
            this.WriteMsg(stream, buffer);
        }
        public virtual void WriteMsg(NetworkStream stream, byte[] buffer)
        {
            if (!stream.CanWrite)
                return;
            stream.WriteTimeout = 1000 * 10;
            stream.Write(buffer, 0, buffer.Length);
            stream.Flush();
        }


        public virtual void WriteMsg_Tx(NetworkStream stream)
        {
            throw new NotImplementedException();
        }
        public virtual void WriteMsg_TxDataReq(NetworkStream stream)
        {
            throw new NotImplementedException();
        }
        public virtual void WriteMsg_TxDataAck(NetworkStream stream)
        {
            throw new NotImplementedException();
        }



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
