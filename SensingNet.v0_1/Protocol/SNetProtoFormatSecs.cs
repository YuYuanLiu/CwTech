using CToolkit.Secs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace SensingNet.v0_1.Protocol
{

    /// <summary>
    /// 客戶要求的Secs Format
    /// </summary>
    public class SNetProtoFormatSecs : ConcurrentQueue<HsmsMessage>, ISNetProtoFormatBase
    {

        HsmsMessageReceiver hsmsMsgRcv = new HsmsMessageReceiver();

        ~SNetProtoFormatSecs() { this.Dispose(false); }




        public void ReceiveBytes(byte[] buffer, int offset, int length)
        {
            this.hsmsMsgRcv.Receive(buffer, offset, length);
        }
        public bool IsReceiving()
        {
            return this.hsmsMsgRcv.GetMsgBufferLength() > 0;
        }
        public bool HasMessage() { return this.Count > 0; }
        public bool TryDequeueMsg(out object msg)
        {
            HsmsMessage mymsg = null;
            var flag = this.TryDequeue(out mymsg);
            msg = mymsg;
            return flag;
        }

    


        #region IDisposable
        // Flag: Has Dispose already been called?
        bool disposed = false;

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
                //
                this.DisposeManaged();
            }

            // Free any unmanaged objects here.
            //
            this.DisposeUnmanaged();
            this.DisposeSelf();
            disposed = true;
        }



        void DisposeManaged()
        {

        }

        void DisposeUnmanaged()
        {

        }

        void DisposeSelf()
        {

        }

        #endregion

    }
}
