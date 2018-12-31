using CToolkit;
using CToolkit.v0_1.NumericProc;
using CToolkit.v0_1.Secs;
using CToolkit.v0_1;
using SensingNet.v0_1.Signal;
using SensingNet.v0_1.Storage;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;

namespace SensingNet.v0_1.QSecs
{
    /// <summary>
    /// 提供一個IP/Port的處理機制
    /// 具Secs被動連線功能
    /// 可將Device收到的資料做簡易處理
    /// 若不需要簡易資料處理, 可自行撰寫, 提供Secs通訊
    /// </summary>
    public class SNetQSecsHandler : ICtkContextFlowRun, IDisposable
    {
        public SNetQSecsCfg cfg;

        public CtkHsmsConnector hsmsConnector;

        /// <summary>
        /// 一個Secs Handler需要一組IP/Port
        /// 對一個 IP/Port 而言, Svid 不應該重複, 因此用 Query SVID 作為Key
        /// </summary>
        public SNetEnumHandlerStatus status = SNetEnumHandlerStatus.None;
        public bool IsWaitDispose;

        #region ICtkContextFlowRun

        public bool CfIsRunning { get; set; }
        public int CfExec()
        {
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
            hsmsConnector.evtReceiveData += delegate(Object sen, CtkHsmsConnector_EventArgsRcvData evt)
            {

                var myMsg = evt.msg;


                //System.Diagnostics.Debug.WriteLine("S{0}F{1}", myMsg.header.StreamId, myMsg.header.FunctionId);
                //System.Diagnostics.Debug.WriteLine("SType= {0}", myMsg.header.SType);

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
        public int CfLoad()
        {
            CtkUtil.RunWorkerAsyn(delegate(object sender, DoWorkEventArgs e)
            {
                for (int idx = 0; !this.disposed; idx++)
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
                }
            });
            return 0;
        }
        public int CfRun() { return 0; }
        public int CfRunAsyn() { return 0; }
        public int CfUnLoad()
        {
            return 0;
        }
        #endregion



        public SNetQSvidCfg GetQSvidCfg(UInt32 svid)
        {
            var query = from row in this.cfg.QSvidCfgList
                        where row.QSvid == svid
                        select row;

            return query.FirstOrDefault();
        }



        #region Event

        public event EventHandler<CtkHsmsConnector_EventArgsRcvData> evtReceiveData;
        public void OnReceiveData(CtkHsmsMessage msg)
        {
            if (this.evtReceiveData == null)
                return;

            this.evtReceiveData(this, new CtkHsmsConnector_EventArgsRcvData() { msg = msg });
        }

        #endregion


        #region Dispose

        bool disposed = false;


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void DisposeManaged()
        {

        }

        public void DisposeSelf()
        {
            if (this.hsmsConnector != null)
            {
                this.hsmsConnector.Dispose();
            }

        }

        public void DisposeUnManaged()
        {
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any managed objects here.
                this.DisposeManaged();
            }

            // Free any unmanaged objects here.
            //
            this.DisposeUnManaged();
            this.DisposeSelf();
            disposed = true;
        }
        #endregion

    }
}
