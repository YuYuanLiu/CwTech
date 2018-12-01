using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensingNet.v0_1.Signal
{
    public class SignalTranSensingNet : ISignalTranBase
    {
        static int ReadData(String[] args, int start, List<double> data)
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
        public SignalEventArgs AnalysisSignal(object msg)
        {
            var line = msg as string;

            var ea = new SignalEventArgs();
            var args = line.Split(new char[] { '\0', ' ' });

            ea.Data = new List<double>();

            for (int idx = 0; idx < args.Length; idx++)
            {
                var arg = args[idx];


                if (args[idx] == "-respData" || args[idx] == "-resp_data")
                {
                    continue;
                }
                else if (args[idx] == "-svid")
                {
                    idx++;
                    if (args.Length <= idx) continue;
                    UInt32.TryParse(args[idx], out ea.Svid);
                    continue;
                }
                else if (args[idx] == "-channel")
                {
                    idx++;
                    if (args.Length <= idx) continue;
                    UInt32.TryParse(args[idx], out ea.Svid);
                    continue;
                }
                else if (args[idx] == "-data")
                {
                    idx = ReadData(args, idx, ea.Data);
                    continue;
                }
            }
            return ea;
        }
    }
}
