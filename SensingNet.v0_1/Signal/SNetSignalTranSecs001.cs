using CToolkit;
using CToolkit.v0_1;
using CToolkit.v0_1.Protocol;
using CToolkit.v0_1.Secs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensingNet.v0_1.Signal
{


    public class SNetSignalTranSecs001 : ISNetSignalTranBase
    {


        public List<SNetSignalEventArgs> AnalysisSignal<T>(object sender, object msg, IList<T> infos)
        {

            var result = new List<SNetSignalEventArgs>();
            var secsMsg = msg as CtkHsmsMessage;

            try
            {
                var list = secsMsg.rootNode as CToolkit.v0_1.Secs.CtkSecsIINodeList;

                for (int idx = 0; idx < list.Data.Count; idx++)
                {

                    var ea = new SNetSignalEventArgs();
                    ea.Sender = sender;
                    var data = list.Data[idx] as CToolkit.v0_1.Secs.CtkSecsIINodeASCII;
                    if (data.Data.Count <= 0) continue;

                    ea.Data = new List<double>();
                    ea.Data.Add(double.Parse(data.GetString()));

                    //this.OnDataTrigger(ea);
                    result.Add(ea);
                }
                return result;

            }
            catch (Exception ex) { CtkLog.Write(ex); }

            return null;
        }

        public CtkProtocolTrxMessage CreateDataReqMsg<T>(IList<T> reqInfos)
        {
            var listInfo = reqInfos as IList<SNetSignalCfg>;
            if (listInfo == null) throw new ArgumentException("未定義此型別的操作方式");


            var txMsg = new CtkHsmsMessage();
            txMsg.header.StreamId = 1;
            txMsg.header.FunctionId = 3;
            txMsg.header.WBit = true;
            var sList = new CtkSecsIINodeList();
            //var sSvid = new CToolkit.v0_1.Secs.SecsIINodeInt64();

            foreach (var scfg in listInfo)
            {
                var sSvid = new CtkSecsIINodeUInt64();
                sSvid.Data.Add(scfg.Svid);
                sList.Data.Add(sSvid);
            }

            txMsg.rootNode = sList;

            return txMsg;


        }


        public CtkProtocolTrxMessage CreateAckMsg<T>(IList<T> reqInfos)
        {
            var listInfo = reqInfos as IList<SNetSignalCfg>;
            if (listInfo == null) throw new ArgumentException("未定義此型別的操作方式");

            return null;
        }
    }
}
