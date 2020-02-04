using CToolkit.v1_0;
using CToolkit.v1_0.Net;
using CToolkit.v1_0.Secs;
using SensingNet.v0_2.QSecs;
using SensingNet.v0_2.TriggerDiagram.Basic;
using SensingNet.v0_2.TimeSignal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;

namespace SensingNet.v0_2.TriggerDiagram
{
    public class SNetTdNQSecs : SNetTdNodeF8
    {
        public SNetQSvidCfg cfg;
        public UInt64 QSvid { get { return this.cfg.QSvid; } }
        public SNetTSignalSetSecF8 TSignal = new SNetTSignalSetSecF8();




        #region Input
        //Do Input 可以有2種做法來確保可以處理最多類型型的資料
        //  1. 使用父類別, 執行時判斷是否是可以處理
        //  2. 使用多型, 不同的資料類型用不同的function處理



        public void Input(object sender, SNetTdSignalSetSecF8EventArg e)
        {
            if (!this.IsEnalbed) return;
            var ea = e as SNetTdSignalSetSecF8EventArg;
            if (ea == null) throw new SNetException("尚無法處理此類資料: " + e.GetType().FullName);


            this.ProcAndPushData(this.TSignal, ea.GetThisOrLast());
            ea.InvokeResult = this.disposed ? SNetTdEnumInvokeResult.IsDisposed : SNetTdEnumInvokeResult.None;
        }



        #endregion


        #region Event

        public event EventHandler<CtkHsmsConnectorRcvDataEventArg> EhReceiveData;
        public void OnReceiveData(CtkHsmsMessage msg)
        {
            if (this.EhReceiveData == null)
                return;

            this.EhReceiveData(this, new CtkHsmsConnectorRcvDataEventArg() { msg = msg });
        }

        #endregion


        #region Dispose

        protected override void DisposeSelf()
        {
            base.DisposeSelf();
        }
        #endregion




    }
}
