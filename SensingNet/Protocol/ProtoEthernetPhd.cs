using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace SensingNet.Protocol
{
    public class ProtoEthernetPhd : ProtoEthernetBase
    {
        public StringBuilder rcvSb = new StringBuilder();
        List<UInt32> rcvBytes = new List<UInt32>();
        Queue<String> cmdQueue = new Queue<String>();


        public override void FirstConnect(Stream stream)
        {
            //this.WriteMsg_Tx(stream);
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
            var result = hasMessage();
            var line = "";
            while (hasMessage())
            {
                lock (this)
                    line = this.cmdQueue.Dequeue();

                var phd = new ProtoEthernetPhdData();

                var args = line.Split(new char[] { '\0', ' ' });

                for (int idx = 0; idx < args.Length; idx++)
                {
                    var arg = args[idx];

                    var temp = arg.Split(new char[] { ':', ' ' });

                    int neg = temp[1].IndexOf("-");
                    if (neg > 0)
                        temp[1] = temp[1].TrimStart(new char[] { '0' });

                    switch (temp[0])
                    {
                        case "CU":
                            double.TryParse(temp[1], out phd.CU);
                            break;
                        case "SA":
                            double.TryParse(temp[1], out phd.SA);
                            break;
                        case "AA":
                            double.TryParse(temp[1], out phd.AA);
                            break;
                        case "BB":
                            double.TryParse(temp[1], out phd.BB);
                            break;
                        case "CC":
                            double.TryParse(temp[1], out phd.CC);
                            break;
                        case "DD":
                            double.TryParse(temp[1], out phd.DD);
                            break;
                        case "P1":
                            double.TryParse(temp[1], out phd.P1);
                            break;
                        case "P2":
                            double.TryParse(temp[1], out phd.P2);
                            break;
                        case "P3":
                            double.TryParse(temp[1], out phd.P3);
                            break;
                        case "TT":
                            double.TryParse(temp[1], out phd.TT);
                            break;
                        case "ID":
                            Int32.TryParse(temp[1], out phd.ID);
                            break;
                        case "TOOL":
                            phd.TOOL = temp[1];
                            break;
                        case "SCU":
                            double.TryParse(temp[1], out phd.SCU);
                            break;
                        case "SSA":
                            double.TryParse(temp[1], out phd.SSA);
                            break;
                        case "SSC":
                            double.TryParse(temp[1], out phd.SSC);
                            break;
                        case "SBB":
                            double.TryParse(temp[1], out phd.SBB);
                            break;
                        case "SCC":
                            double.TryParse(temp[1], out phd.SCC);
                            break;
                        case "SDD":
                            double.TryParse(temp[1], out phd.SDD);
                            break;
                        default:
                            continue;
                    }
                }//end for



                if (phd.TOOL == null) continue;

                var toolids = this.dConfig.ToolId.Split(new char[] { ',' });

                var tool_idx = 0;
                for (tool_idx = 0; tool_idx < toolids.Length; tool_idx++)
                {
                    if (toolids[tool_idx] == phd.TOOL)
                        break;
                }


                {
                    var ea = new ProtoEventArgs();
                    ea.DeviceSvid = (uint)(1 + tool_idx * 65535);
                    ea.Data.Add(phd.CU);
                    ea.ToolId = phd.TOOL;
                    this.OnDataTrigger(ea);
                }
                {
                    var ea = new ProtoEventArgs();
                    ea.DeviceSvid = (uint)(2 + tool_idx * 65535);
                    ea.Data.Add(phd.SA);
                    ea.ToolId = phd.TOOL;
                    this.OnDataTrigger(ea);
                }
                {
                    var ea = new ProtoEventArgs();
                    ea.DeviceSvid = (uint)(3 + tool_idx * 65535);
                    ea.Data.Add(phd.AA);
                    ea.ToolId = phd.TOOL;
                    this.OnDataTrigger(ea);
                }
                {
                    var ea = new ProtoEventArgs();
                    ea.DeviceSvid = (uint)(4 + tool_idx * 65535);
                    ea.Data.Add(phd.BB);
                    ea.ToolId = phd.TOOL;
                    this.OnDataTrigger(ea);
                }
                {
                    var ea = new ProtoEventArgs();
                    ea.DeviceSvid = (uint)(5 + tool_idx * 65535);
                    ea.Data.Add(phd.CC);
                    ea.ToolId = phd.TOOL;
                    this.OnDataTrigger(ea);
                }
                {
                    var ea = new ProtoEventArgs();
                    ea.DeviceSvid = (uint)(6 + tool_idx * 65535);
                    ea.Data.Add(phd.DD);
                    ea.ToolId = phd.TOOL;
                    this.OnDataTrigger(ea);
                }
                {
                    var ea = new ProtoEventArgs();
                    ea.DeviceSvid = (uint)(7 + tool_idx * 65535);
                    ea.Data.Add(phd.P1);
                    ea.ToolId = phd.TOOL;
                    this.OnDataTrigger(ea);
                }
                {
                    var ea = new ProtoEventArgs();
                    ea.DeviceSvid = (uint)(8 + tool_idx * 65535);
                    ea.Data.Add(phd.P2);
                    ea.ToolId = phd.TOOL;
                    this.OnDataTrigger(ea);
                }
                {
                    var ea = new ProtoEventArgs();
                    ea.DeviceSvid = (uint)(9 + tool_idx * 65535);
                    ea.Data.Add(phd.P3);
                    ea.ToolId = phd.TOOL;
                    this.OnDataTrigger(ea);
                }
                {
                    var ea = new ProtoEventArgs();
                    ea.DeviceSvid = (uint)(10 + tool_idx * 65535);
                    ea.Data.Add(phd.TT);
                    ea.ToolId = phd.TOOL;
                    this.OnDataTrigger(ea);
                }

                {
                    var ea = new ProtoEventArgs();
                    ea.DeviceSvid = (uint)(11 + tool_idx * 65535);
                    ea.Data.Add(phd.SCU);
                    ea.ToolId = phd.TOOL;
                    this.OnDataTrigger(ea);
                }

                {
                    var ea = new ProtoEventArgs();
                    ea.DeviceSvid = (uint)(12 + tool_idx * 65535);
                    ea.Data.Add(phd.SSA);
                    ea.ToolId = phd.TOOL;
                    this.OnDataTrigger(ea);
                }

                {
                    var ea = new ProtoEventArgs();
                    ea.DeviceSvid = (uint)(13 + tool_idx * 65535);
                    ea.Data.Add(phd.SSC);
                    ea.ToolId = phd.TOOL;
                    this.OnDataTrigger(ea);
                }

                {
                    var ea = new ProtoEventArgs();
                    ea.DeviceSvid = (uint)(14 + tool_idx * 65535);
                    ea.Data.Add(phd.SBB);
                    ea.ToolId = phd.TOOL;
                    this.OnDataTrigger(ea);
                }
                {
                    var ea = new ProtoEventArgs();
                    ea.DeviceSvid = (uint)(15 + tool_idx * 65535);
                    ea.Data.Add(phd.SCC);
                    ea.ToolId = phd.TOOL;
                    this.OnDataTrigger(ea);
                }
                {
                    var ea = new ProtoEventArgs();
                    ea.DeviceSvid = (uint)(16 + tool_idx * 65535);
                    ea.Data.Add(phd.SDD);
                    ea.ToolId = phd.TOOL;
                    this.OnDataTrigger(ea);
                }

            }

            return result;
        }





        public override void WriteMsg_Tx(Stream stream)
        {
            /*
            if (this.dConfig.IsActivelyTx)
                this.WriteMsg_TxDataAck(stream);
            else
                this.WriteMsg_TxDataReq(stream);*/
        }

        public override void WriteMsg_TxDataReq(Stream stream)
        {
            //this.WriteMsg(stream, TxReqMsg);
        }
        public override void WriteMsg_TxDataAck(Stream stream)
        {
            //this.WriteMsg(stream, TxReqAck);
        }















        //=== Static ======================================================

        public static byte[] TxReqMsg = Encoding.UTF8.GetBytes("cmd -reqData \n");
        public static byte[] TxReqAck = Encoding.UTF8.GetBytes("\n");//減少處理量, 只以換行作為Ack



    }
}
