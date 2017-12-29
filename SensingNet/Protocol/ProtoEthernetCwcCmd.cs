using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace SensingNet.Protocol
{
    public class ProtoEthernetCwcCmd : ProtoEthernetBase
    {
        StringBuilder rcvSb = new StringBuilder();
        List<UInt32> rcvBytes = new List<UInt32>();
        Queue<String> cmdQueue = new Queue<String>();


        public override void FirstConnect(Stream stream)
        {
            this.WriteMsg_Tx(stream);
        }

        public override void ReceiveBytes(byte[] buffer, int offset, int length)
        {
            lock (this)
                this.rcvSb.Append(Encoding.UTF8.GetString(buffer, offset, length));

            var line = "";
            using (var sr = new StringReader(rcvSb.ToString()))
            {
                for (line = sr.ReadLine(); line != null; line = sr.ReadLine())
                {
                    //if (line.IndexOf("\n") < 0) break;

                    line = line.Replace("\r", "");
                    line = line.Replace("\n", "");
                    line = line.Trim();
                    lock (this)
                        this.cmdQueue.Enqueue(line);
                    line = "";
                }
            }
            this.rcvSb.Clear();
            lock (this)
                this.rcvSb.Append(line);
        }
        public override bool IsReceiving()
        {
            return this.rcvSb.Length > 0;
        }
        public override bool hasMessage()
        {
            return this.cmdQueue.Count > 0;
        }



        public override bool AnalysisData(Stream stream)
        {
            var result = this.cmdQueue.Count > 0;
            var line = "";
            while (this.cmdQueue.Count > 0)
            {
                lock (this)
                    line = this.cmdQueue.Dequeue();

                var ea = new ProtoEventArgs();
                var args = line.Split(new char[] { '\0', ' ' });

                ea.Data = new List<double>();

                for (int idx = 0; idx < args.Length; idx++)
                {
                    var arg = args[idx];


                    if (args[idx] == "-respData")
                    {
                        continue;
                    }
                    else if (args[idx] == "-svid")
                    {
                        idx++;
                        if (args.Length <= idx) continue;
                        UInt32.TryParse(args[idx], out ea.DeviceSvid);
                        continue;
                    }
                    else if (args[idx] == "-channel")
                    {
                        idx++;
                        if (args.Length <= idx) continue;
                        UInt32.TryParse(args[idx], out ea.DeviceSvid);
                        continue;
                    }
                    else if (args[idx] == "-data")
                    {
                        idx = ReadData(args, idx, ea.Data);
                        continue;
                    }
                }

                if (ea.Data.Count > 0)
                {
                    this.OnDataTrigger(ea);
                }
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
            this.WriteMsg(stream, TxReqMsg);
        }
        public override void WriteMsg_TxDataAck(Stream stream)
        {
            this.WriteMsg(stream, TxReqAck);
        }




        int ReadData(String[] args, int start, List<double> data)
        {
            //讀取資料, 皆為double, 否則視為結束
            //return 最後一筆資料的索引

            var d = 0.0;
            //第一筆為 -reqData
            int idx = 0;
            for (idx = start + 1; idx < args.Length; idx++)
            {
                if (Double.TryParse(args[idx], out d))
                    data.Add(d);
                else
                    break;
            }

            return idx - 1;
        }











        //=== Static ======================================================

        public static byte[] TxReqMsg = Encoding.UTF8.GetBytes("cmd -reqData \n");
        public static byte[] TxReqAck = Encoding.UTF8.GetBytes("\n");//減少處理量, 只以換行作為Ack

        


    }
}
