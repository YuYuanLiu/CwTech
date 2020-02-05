﻿using CodeExpress.v1_0.Secs;
using CToolkit.v1_0;
using CToolkit.v1_0.Net;
using CToolkit.v1_0.Threading;
using SensingNet.v0_2.QSecs;
using SensingNet.v0_2.TriggerDiagram.Basic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;

namespace SensingNet.v0_2.TriggerDiagram
{
    public class SNetTdBQSecs : SNetTdBlock, ICtkContextFlowRun
    {
        public SNetQSecsCfg cfg;
        public CxHsmsConnector hsmsConnector;

        #region ICtkContextFlowRun

        public bool CfIsRunning { get; set; }
        public int CfExec()
        {
            try
            {
                hsmsConnector.Connect();
                hsmsConnector.ReceiveRepeat();
            }
            catch (Exception ex)
            {
                CtkLog.Write(ex);
            }
            finally { System.Threading.Thread.Sleep(1000); }
            return 0;
        }
        public int CfFree()
        {
            this.Dispose(false);
            return 0;
        }
        public int CfInit()
        {
            hsmsConnector = new CxHsmsConnector();
            //hsmsConnector.ctkConnSocket.isActively = true;

            var localUri = new Uri(this.cfg.LocalUri);
            var remoteUri = new Uri(this.cfg.RemoteUri);

            var localIp = CtkNetUtil.GetLikelyFirst127Ip(localUri.Host, remoteUri.Host);
            if (localIp == null) throw new Exception("無法取得在地IP");
            hsmsConnector.local = new IPEndPoint(localIp, localUri.Port);
            hsmsConnector.EhReceiveData += delegate (Object sen, CxHsmsConnectorRcvDataEventArg ea)
            {

                var myMsg = ea.msg;

                switch (myMsg.header.SType)
                {
                    case 1:
                        hsmsConnector.Send(CxHsmsMessage.CtrlMsg_SelectRsp(0));
                        return;
                    case 5:
                        hsmsConnector.Send(CxHsmsMessage.CtrlMsg_LinktestRsp());
                        return;
                }

                this.OnReceiveData(myMsg);

            };



            return 0;
        }
        public int CfLoad() { return 0; }
        public int CfRun()
        {
            while (!this.disposed && this.CfIsRunning)
            {
                this.CfExec();
            }
            return 0;
        }
        public int CfRunAsyn()
        {
            this.CfIsRunning = true;
            CtkThreadingUtil.RunWorkerAsyn(delegate (object sender, DoWorkEventArgs e)
            {
                this.CfRun();
            });
            return 0;
        }
        public int CfUnLoad() { return 0; }
        #endregion



        public SNetQSvidCfg GetQSvidCfg(UInt32 svid)
        {
            var query = from row in this.cfg.QSvidCfgList
                        where row.QSvid == svid
                        select row;

            return query.FirstOrDefault();
        }


        #region Input

        public void Input(object sender, SNetTdSignalEventArg e)
        {
            if (!this.IsEnalbed) return;
            var ea = e as SNetTdSignalSecF8EventArg;
            if (ea == null) throw new SNetException("尚無法處理此類資料: " + e.GetType().FullName);

            ea.InvokeResult = this.disposed ? SNetTdEnumInvokeResult.IsDisposed : SNetTdEnumInvokeResult.None;
        }

        #endregion


        #region Event

        public event EventHandler<CxHsmsConnectorRcvDataEventArg> EhReceiveData;
        public void OnReceiveData(CxHsmsMessage msg)
        {
            if (this.EhReceiveData == null)
                return;

            this.EhReceiveData(this, new CxHsmsConnectorRcvDataEventArg() { msg = msg });
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
