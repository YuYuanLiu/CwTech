using CToolkit.Secs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace SensingNet.v0_0.Protocol
{
    public class ProtoSecs : ProtoBase, IDisposable
    {

        CToolkit.Secs.HsmsMessageReceiver hsmsMsgRcv = new CToolkit.Secs.HsmsMessageReceiver();


        ~ProtoSecs() { this.Dispose(false); }



        public override void FirstConnect(Stream stream)
        {
            var txMsg = HsmsMessage.CtrlMsg_SelectReq();
            this.WriteMsg(stream, txMsg.ToBytes());
            txMsg = HsmsMessage.CtrlMsg_LinktestReq();
            this.WriteMsg(stream, txMsg.ToBytes());


        }


        public override void ReceiveBytes(byte[] buffer, int offset, int length)
        {
            this.hsmsMsgRcv.Receive(buffer, offset, length);
        }

        public override bool IsReceiving()
        {
            return this.hsmsMsgRcv.GetMsgBufferLength() > 0;
        }
        public override bool hasMessage()
        {
            return this.hsmsMsgRcv.Count > 0;
        }

        public override bool AnalysisData(Stream stream)
        {
            var result = this.hsmsMsgRcv.Count > 0;

            while (hsmsMsgRcv.Count > 0)
            {
                var msg = hsmsMsgRcv.Dequeue();


                switch (msg.header.SType)
                {
                    case 1:
                        this.WriteMsg(stream, HsmsMessage.CtrlMsg_SelectRsp(0).ToBytes());
                        continue;
                    case 2:
                        continue;
                    case 5:
                        this.WriteMsg(stream, HsmsMessage.CtrlMsg_LinktestRsp().ToBytes());
                        continue;
                    case 6:
                        continue;
                }




                try
                {
                    var list = msg.rootNode as CToolkit.Secs.SecsIINodeList;

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
                    }

                }
                catch (Exception ex) { LoggerAssembly.Write(ex); }
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

            var txMsg = new CToolkit.Secs.HsmsMessage();
            txMsg.header.StreamId = 1;
            txMsg.header.FunctionId = 3;
            txMsg.header.WBit = true;
            var sList = new CToolkit.Secs.SecsIINodeList();
            //var sSvid = new CToolkit.Secs.SecsIINodeInt64();

            foreach (var scfg in this.dConfig.SignalCfgList)
            {
                var sSvid = new CToolkit.Secs.SecsIINodeUInt32();
                sSvid.Data.Add(scfg.DeviceSvid);
                sList.Data.Add(sSvid);
            }

            txMsg.rootNode = sList;
            this.WriteMsg(stream, txMsg.ToBytes());

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
