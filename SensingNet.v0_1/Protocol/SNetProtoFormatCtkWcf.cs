using CToolkit.v1_0.Protocol;
using CToolkit.v1_0.Secs;
using CToolkit.v1_0.Wcf;
using CToolkit.v1_0.Wcf.DuplexTcp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SensingNet.v0_1.Protocol
{

    /// <summary>
    /// 客戶要求的Secs Format
    /// </summary>
    public class SNetProtoFormatCtkWcf : ISNetProtoFormatBase, IDisposable
    {

        public ConcurrentQueue<CtkWcfMessage> MsgQueue = new ConcurrentQueue<CtkWcfMessage>();
        ManualResetEvent mre = new ManualResetEvent(false);

        ~SNetProtoFormatCtkWcf() { this.Dispose(false); }


        #region ISNetProtoFormatBase

        int ISNetProtoFormatBase.Count() { return this.MsgQueue.Count; }

        public bool HasMessage() { return this.MsgQueue.Count > 0; }

        public bool IsReceiving() { return this.mre.WaitOne(1000); }

        public void ReceiveMsg(CtkProtocolTrxMessage msg)
        {
            try
            {
                mre.Reset();
                if (msg.Is<CtkWcfMessage>())
                    this.MsgQueue.Enqueue(msg.As<CtkWcfMessage>());
                else
                    throw new ArgumentException("Not support type");
            }
            finally { mre.Set(); }

        }


        public bool TryDequeueMsg(out object msg)
        {
            CtkWcfMessage mymsg = null;
            var flag = this.MsgQueue.TryDequeue(out mymsg);
            msg = mymsg;
            return flag;
        }
        #endregion



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
            }

            // Free any unmanaged objects here.
            //
            this.DisposeSelf();
            disposed = true;
        }






        void DisposeSelf()
        {

        }




        #endregion

    }
}
