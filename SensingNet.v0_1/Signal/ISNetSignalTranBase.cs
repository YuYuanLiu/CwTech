using CToolkit.v0_1.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensingNet.v0_1.Signal
{
    /// <summary>
    /// 資料的分析處理
    /// Protocol 解譯後, 要如何處理使用資料
    /// </summary>
    public interface ISNetSignalTranBase
    {

        List<SNetSignalEventArgs> AnalysisSignal<T>(object sender, object msg, IList<T> infos);
        CtkProtocolTrxMessage CreateDataReqMsg<T>(IList<T> reqInfos);
        CtkProtocolTrxMessage CreateAckMsg<T>(IList<T> reqInfos);


    }
}
