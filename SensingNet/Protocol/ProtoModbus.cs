using CToolkit.Modbus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace SensingNet.Protocol
{
    public class ProtoModbus : ProtoBase, IDisposable
    {

        ModbusMessageReceiver msgReceiver = new ModbusMessageReceiver();


        ~ProtoModbus() { this.Dispose(false); }



        public override void FirstConnect(Stream stream)
        {
     

        }


        public override void ReceiveBytes(byte[] buffer, int offset, int length)
        {
            this.msgReceiver.Receive(buffer, offset, length);
        }

        public override bool IsReceiving()
        {
            return this.msgReceiver.GetMsgBufferLength() > 0;
        }
        public override bool hasMessage()
        {
            return this.msgReceiver.Count > 0;
        }

        public override bool AnalysisData(Stream stream)
        {
            var result = this.msgReceiver.Count > 0;

            while (msgReceiver.Count > 0)
            {
                var msg = msgReceiver.Dequeue();

                //TODO: Data Receive

                /*
                var list = msg;

                for (int idx = 0;
                    idx < list.Data.Count && idx < this.dConfig.SignalCfgList.Count;
                    idx++)
                {

                    var ea = new SignalEventArgs();
                    var scfg = this.dConfig.SignalCfgList[idx];
                    ea.DeviceSvid = scfg.DeviceSvid;

                    var data = list.Data[idx] as CToolkit.Secs.SecsIINodeASCII;
                    if (data.Data.Count <= 0) continue;

                    ea.Data = new List<double>();
                    ea.Data.Add(double.Parse(data.GetString()));

                    this.OnDataTrigger(ea);
                }*/

            }

            return result;

        }

        public override void WriteMsg_Tx(Stream stream)
        {
            if (this.dConfig.IsActivelyTx)
                this.WriteMsg_TxDataAck(stream);
            else
                this.WriteMsg_TxDataReq(stream);
        }
        public override void WriteMsg_TxDataReq(Stream stream)
        {

            var txMsg = new ModbusMessage();

            //TODO: Data request

            this.WriteMsg(stream, txMsg.ToRequestBytes());

        }
        public override void WriteMsg_TxDataAck(Stream stream)
        {
            throw new NotImplementedException("目前不存在主動SECS傳送的Device");
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
