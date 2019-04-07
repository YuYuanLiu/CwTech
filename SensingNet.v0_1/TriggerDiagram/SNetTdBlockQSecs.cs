using CToolkit.v1_0;
using CToolkit.v1_0.Net;
using CToolkit.v1_0.Secs;
using SensingNet.v0_1.QSecs;
using SensingNet.v0_1.TriggerDiagram.Basic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;

namespace SensingNet.v0_1.TriggerDiagram
{
    public class SNetTdBlockQSecs : SNetTdBlock, ICtkContextFlowRun
    {
        public SNetQSecsCfg cfg;
        public CtkHsmsConnector hsmsConnector;

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
            hsmsConnector = new CtkHsmsConnector();
            //hsmsConnector.ctkConnSocket.isActively = true;
            var localIp = CtkNetUtil.GetLikelyFirst127Ip(this.cfg.LocalIp, this.cfg.RemoteIp);
            if (localIp == null) throw new Exception("無法取得在地IP");
            hsmsConnector.local = new IPEndPoint(localIp, this.cfg.LocalPort);
            hsmsConnector.evtReceiveData += delegate (Object sen, CtkHsmsConnectorRcvDataEventArg evt)
            {

                var myMsg = evt.msg;

                switch (myMsg.header.SType)
                {
                    case 1:
                        hsmsConnector.Send(CtkHsmsMessage.CtrlMsg_SelectRsp(0));
                        return;
                    case 5:
                        hsmsConnector.Send(CtkHsmsMessage.CtrlMsg_LinktestRsp());
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
            CtkUtil.RunWorkerAsyn(delegate (object sender, DoWorkEventArgs e)
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


        #region Do

        public void DoInput(object sender, SNetTdSignalEventArg e)
        {
            if (!this.IsEnalbed) return;
            var ea = e as SNetTdSignalSecSetF8EventArg;
            if (ea == null) throw new SNetException("尚無法處理此類資料: " + e.GetType().FullName);

            ea.InvokeResult = this.disposed ? SNetTdEnumInvokeResult.IsDisposed : SNetTdEnumInvokeResult.None;
        }

        #endregion


        #region Event

        public event EventHandler<CtkHsmsConnectorRcvDataEventArg> evtReceiveData;
        public void OnReceiveData(CtkHsmsMessage msg)
        {
            if (this.evtReceiveData == null)
                return;

            this.evtReceiveData(this, new CtkHsmsConnectorRcvDataEventArg() { msg = msg });
        }

        #endregion


        #region Dispose
        bool disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        public void DisposeSelf()
        {
            if (this.hsmsConnector != null)
            {
                this.hsmsConnector.Dispose();
            }

        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any managed objects here.
            }

            // Free any unmanaged objects here.
            //
            this.DisposeSelf();
            disposed = true;
        }
        #endregion




    }
}
