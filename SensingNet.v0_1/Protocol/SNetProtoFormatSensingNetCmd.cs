using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace SensingNet.v0_1.Protocol
{
    public class SNetProtoFormatSensingNetCmd : ConcurrentQueue<string>, ISNetProtoFormatBase, IDisposable
    {
   
        StringBuilder rcvSb = new StringBuilder();



        #region IProtoBase


        public void ReceiveBytes(byte[] buffer, int offset, int length)
        {
            lock (this)
            {
                this.rcvSb.Append(Encoding.UTF8.GetString(buffer, offset, length));
                var content = this.rcvSb.ToString();
                for (var idx = content.IndexOf('\n'); idx >= 0; idx = content.IndexOf('\n'))
                {
                    var line = content.Substring(0, idx + 1);
                    line = line.Replace("\r", "");
                    line = line.Replace("\n", "");
                    line = line.Trim();
                    if (line.Contains("cmd"))
                        this.Enqueue(line);
                    content = content.Remove(0, idx + 1);
                }
                this.rcvSb.Clear();
                this.rcvSb.Append(content);
            }
        }
        public bool IsReceiving()
        {
            return this.rcvSb.Length > 0;
        }
        public bool HasMessage()
        {
            return this.Count > 0;
        }
        public bool TryDequeueMsg(out object msg)
        {
            string line = null;
            var flag = this.TryDequeue(out line);
            msg = line;
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
