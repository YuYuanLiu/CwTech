using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace SensingNet.v0_1.Protocol
{
    public class ProtoSensingNetCmd : ConcurrentQueue<string>, IProtoBase, IDisposable
    {
        public static byte[] TxDataReq = Encoding.UTF8.GetBytes("cmd -reqData \n");
        public static byte[] TxDataAck = Encoding.UTF8.GetBytes("\n");//減少處理量, 只以換行作為Ack

        StringBuilder rcvSb = new StringBuilder();



        #region IProtoBase

        public void FirstConnect(Stream stream)
        {

        }
        public void ReceiveBytes(byte[] buffer, int offset, int length)
        {
            lock (this)
            {
                this.rcvSb.Append(Encoding.UTF8.GetString(buffer, offset, length));
                var content = this.rcvSb.ToString();
                for (var idx = content.IndexOf('\n'); idx >= 0; idx = content.IndexOf('\n'))
                {
                    var line = content.Substring(0, idx);
                    line = line.Replace("\r", "");
                    line = line.Replace("\n", "");
                    line = line.Trim();
                    this.Enqueue(line);
                    content = content.Remove(0, idx);
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

        public void WriteMsg(Stream stream, string msg) { this.WriteMsg(stream, Encoding.UTF8.GetBytes(msg)); }
        public void WriteMsg(Stream stream, byte[] buffer) { stream.Write(buffer, 0, buffer.Length); }
        public void WriteMsgDataReq(Stream stream)
        {
            this.WriteMsg(stream, TxDataReq);
        }
        public void WriteMsgDataAck(Stream stream)
        {
            this.WriteMsg(stream, TxDataAck);
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
