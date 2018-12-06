using CToolkit;
using CToolkit.NumericProc;
using CToolkit.Secs;
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
    public class SNetQSecsHandler : IContextFlowRun, IDisposable
    {
        public SNetQSecsCfg cfg;

        /// <summary>
        /// 一個Secs Handler需要一組IP/Port
        /// 對一個 IP/Port 而言, Svid 不應該重複, 因此用 Query SVID 作為Key
        /// </summary>
        public Dictionary<UInt32, SNetQSecsHandlerSvidData> qsvidSignalDataDict = new Dictionary<UInt32, SNetQSecsHandlerSvidData>();



        public HsmsConnector hsmsConnector;
        public SNetEnumHandlerStatus status = SNetEnumHandlerStatus.None;
        public bool WaitDispose;



        public bool CfIsRunning { get; set; }
        public int CfInit()
        {
            hsmsConnector = new HsmsConnector();
            //hsmsConnector.ctkConnSocket.isActively = true;
            var localIp = NetUtil.GetLikelyFirst127Ip(this.cfg.LocalIp, this.cfg.RemoteIp);
            if (localIp == null) throw new Exception("無法取得在地IP");
            hsmsConnector.local = new IPEndPoint(localIp, this.cfg.LocalPort);
            hsmsConnector.evtReceiveData += delegate (Object sen, HsmsConnector_EventArgsRcvData evt)
            {

                var myMsg = evt.msg;


                //System.Diagnostics.Debug.WriteLine("S{0}F{1}", myMsg.header.StreamId, myMsg.header.FunctionId);
                //System.Diagnostics.Debug.WriteLine("SType= {0}", myMsg.header.SType);

                switch (myMsg.header.SType)
                {
                    case 1:
                        hsmsConnector.Send(HsmsMessage.CtrlMsg_SelectRsp(0));
                        return;
                    case 5:
                        hsmsConnector.Send(HsmsMessage.CtrlMsg_LinktestRsp());
                        return;
                }

                this.OnReceiveData(myMsg);

            };



            return 0;
        }
        public int CfLoad()
        {
            CToolkit.CtkUtil.RunWorkerAsyn(delegate (object sender, DoWorkEventArgs e)
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
        public int CfExec()
        {
            return 0;
        }
        public int CfRun() { return 0; }
        public int CfRunAsyn() { return 0; }
        public int CfUnLoad()
        {
            return 0;
        }
        public int CfFree()
        {
            this.Dispose(false);
            return 0;
        }




        /// <summary>
        /// 簡易資料處理
        /// 當收到訊號時, 可以執行此函式, 更新資料
        /// </summary>
        /// <param name="sea"></param>
        public void DoRcvSignalData(SNetSignalEventArgs sea)
        {
            foreach (var qsvidcfg in this.cfg.QSvidCfgList)
            {
                // TODO: 太久沒更新的資料要清掉, 避免佔記憶體空間


                var flag = false;
                // IP / Port 有對應的優先
                if (qsvidcfg.DeviceIp == sea.RemoteIp && qsvidcfg.DevicePort == sea.RemotePort)
                    flag = true;

                // 否則要對應 Device Name
                if (!flag && qsvidcfg.DeviceName == sea.Name)
                    flag = true;

                //需要對應SVID
                flag &= qsvidcfg.DeviceSvid == sea.Svid;

                if (!flag) continue;


                var qsvidData = this.GetSignalCollector(qsvidcfg.QSvid);
                var sps = (from s in qsvidData.SignalCollector
                           where s.dt.ToString("yyyyMMddHHmmss") == sea.RcvDateTime.ToString("yyyyMMddHHmmss")
                           select s).FirstOrDefault();
                if (sps == null)
                {
                    sps = new Storage.SNetSignalPerSec();
                    qsvidData.SignalCollector.AddLast(sps);
                    sps.dt = sea.RcvDateTime;
                }

                IEnumerable<double> signalData = sea.CalibrateData;
                if (qsvidcfg.PassFilter != EnumPassFilter.None)
                {
                    qsvidData.InitFilterIfNull(qsvidcfg.PassFilter, qsvidcfg.PassFilter_SampleRate, qsvidcfg.PassFilter_CutoffLow, qsvidcfg.PassFilter_CutoffHigh);
                    signalData = CToolkit.NumericProc.NpUtil.Interpolation(signalData, (int)qsvidcfg.PassFilter_SampleRate);
                    signalData = qsvidData.ProcessSamples(signalData);
                }
                sps.signals.AddRange(signalData);


                while (qsvidData.SignalCollector.Count > qsvidcfg.StatisticsSecond)
                {
                    qsvidData.SignalCollector.RemoveFirst();
                    if (this.qsvidSignalDataDict.Count <= 0) break;
                }
            }


        }



        public bool StatisticsValue(UInt32 qsvid, SNetSignalCollector signalCollector, out double val)
        {
            val = 0;
            foreach (var qsvidcfg in this.cfg.QSvidCfgList)
            {
                if (qsvidcfg.QSvid != qsvid) continue;

                signalCollector.RefreshTime();
                if (signalCollector.Count == 0) return false;
                switch (qsvidcfg.StatisticsMethod)
                {
                    case SNetEnumStatisticsMethod.Max:
                        val = signalCollector.signals.Max();
                        return true;
                    case SNetEnumStatisticsMethod.Min:
                        val = signalCollector.signals.Min();
                        return true;
                    case SNetEnumStatisticsMethod.Average:
                        val = signalCollector.signals.Average();
                        return true;
                }
            }

            return false;
        }

        public bool StatisticsValue(UInt32 qsvid, out double val)
        {
            val = 0;
            foreach (var qsvidcfg in this.cfg.QSvidCfgList)
            {
                if (qsvidcfg.QSvid != qsvid) continue;

                var qsvidData = this.GetSignalCollector(qsvidcfg.QSvid);
                var collector = qsvidData.SignalCollector;
                return this.StatisticsValue(qsvid, collector, out val);
            }
            return false;
        }


        public double StatisticsValue(UInt32 qsvid)
        {
            var val = 0.0;
            this.StatisticsValue(qsvid, out val);
            return val;
        }





        public SNetQSecsHandlerSvidData GetSignalCollector(UInt32 qsvid)
        {
            if (!this.qsvidSignalDataDict.ContainsKey(qsvid))
                this.qsvidSignalDataDict[qsvid] = new SNetQSecsHandlerSvidData();
            return this.qsvidSignalDataDict[qsvid];
        }
        public SNetQSvidCfg GetQSvidCfg(UInt32 qsvid)
        {
            foreach (var qsvidcfg in this.cfg.QSvidCfgList)
            {
                if (qsvidcfg.QSvid != qsvid) continue;
                return qsvidcfg;
            }

            return null;
        }

        #region Event

        public event EventHandler<HsmsConnector_EventArgsRcvData> evtReceiveData;
        public void OnReceiveData(HsmsMessage msg)
        {
            if (this.evtReceiveData == null)
                return;

            this.evtReceiveData(this, new HsmsConnector_EventArgsRcvData() { msg = msg });
        }

        #endregion


        #region Dispose

        bool disposed = false;


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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

        public void DisposeManaged()
        {

        }

        public void DisposeUnManaged()
        {
        }

        public void DisposeSelf()
        {
            if (this.hsmsConnector != null)
            {
                this.hsmsConnector.Dispose();
            }

        }

        #endregion

    }
}
