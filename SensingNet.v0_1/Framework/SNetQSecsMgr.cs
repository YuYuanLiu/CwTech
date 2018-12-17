using CToolkit;
using CToolkit.Logging;
using CToolkit.Secs;
using SensingNet.v0_1.QSecs;
using SensingNet.v0_1.Signal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SensingNet.v0_1.Framework
{
    public class SNetQSecsMgr : IContextFlowRun, IDisposable
    {
        public String DefaultConfigsFolder = "Config/QSecsConfigs/";
        public CToolkit.Config.ConfigCollector<SNetQSecsCfg> configs = new CToolkit.Config.ConfigCollector<SNetQSecsCfg>();
        public Dictionary<String, SNetQSecsHandler> handlers = new Dictionary<String, SNetQSecsHandler>();
        Task<int> runTask;


        ~SNetQSecsMgr() { this.Dispose(false); }



        public bool CfIsRunning { get; set; }
        public int CfInit()
        {
            this.configs.UpdateFromFolder(DefaultConfigsFolder);


            return 0;
        }
        public int CfLoad()
        {
            return 0;
        }
        public int CfExec()
        {
            try
            {
                if (!Monitor.TryEnter(this, 5 * 1000)) return -1;

                this.configs.UpdateIfOverTime();
                this.RunHandlerStatus();
            }
            finally
            {
                Monitor.Exit(this);
            }
            return 0;
        }
        public int CfRun()
        {
            this.CfIsRunning = true;
            while (!this.disposed && this.CfIsRunning)
            {
                try
                {
                    this.CfExec();
                    Thread.Sleep(1000);
                }
                catch (Exception ex) { CtkLog.Write(ex); }
            }

            return 0;
        }
        public int CfRunAsyn()
        {
            if (this.runTask != null)
                if (!this.runTask.Wait(100)) return 0;//正在工作

            this.runTask = Task.Factory.StartNew<int>(() => this.CfRun());
            return 0;
        }
        public int CfUnLoad()
        {
            this.configs.ClearAll();
            this.RunHandlerStatus();
            return 0;
        }
        public int CfFree()
        {
            this.configs.ClearAll();
            this.RunHandlerStatus();
            this.Dispose(false);
            return 0;
        }

            /// <summary>
        /// 簡易資料處理的更新
        /// 當收到訊號欲更新各QSecsHandler時, 執行此函式
        /// 想要自行處理資料, 可以忽略
        /// </summary>
        public void DoSecsDataUpdate(SNetSignalEventArgs sea)
        {
            try
            {
                if (!Monitor.TryEnter(this, 5 * 1000)) return;
                this.configs.UpdateIfOverTime();


                //廣播
                foreach (var dict in this.configs)
                    foreach (var qsecscfg in dict.Value)
                    {
                        if (!handlers.ContainsKey(qsecscfg.Key))
                            handlers[qsecscfg.Key] = new SNetQSecsHandler();

                        var sh = handlers[qsecscfg.Key];
                        sh.cfg = qsecscfg.Value;

                        //執行, 有相關的Handler自己處理
                        sh.DoRcvSignalData(sea);
                    }
            }
            finally
            {
                Monitor.Exit(this);
            }

        }



        void RunHandlerStatus()
        {
            if (this.disposed) return;

            //先全部設定為: 等待Dispose
            foreach (var hdl in this.handlers)
            {
                hdl.Value.WaitDispose = true;
            }


            //Run過所有Config
            //有Config的會解除等待Dispoe
            //有Config的會執行CfRun
            foreach (var dict in this.configs)
                foreach (var cfg in dict.Value)
                {
                    SNetQSecsHandler hdl = null;
                    if (!this.handlers.ContainsKey(cfg.Key))
                    {
                        hdl = new SNetQSecsHandler();
                        this.handlers.Add(cfg.Key, hdl);
                    }
                    else { hdl = this.handlers[cfg.Key]; }
                    hdl.cfg = cfg.Value;

                    //解除等待Dispoe
                    hdl.WaitDispose = false;

                    if (hdl.status == SNetEnumHandlerStatus.None)
                    {
                        hdl.CfInit();
                        hdl.evtReceiveData += delegate (object ss, HsmsConnector_EventArgsRcvData ea)
                        {
                            this.OnReceiveData(new SNetQSecsRcvDataEventArgs()
                            {
                                handler = ss as SNetQSecsHandler,
                                message = ea.msg
                            });
                        };
                        hdl.status = SNetEnumHandlerStatus.Init;
                    }


                    if (hdl.status == SNetEnumHandlerStatus.Init)
                    {
                        hdl.CfLoad();
                        hdl.status = SNetEnumHandlerStatus.Load;
                    }

                    //有Config的持續作業
                    if (hdl.status == SNetEnumHandlerStatus.Load || hdl.status == SNetEnumHandlerStatus.Run)
                    {

                        hdl.status = SNetEnumHandlerStatus.Run;
                        hdl.CfRun();
                    }

                }


            //沒有Config的會關閉
            var removeHandlers = new Dictionary<String, SNetQSecsHandler>();
            foreach (var qsh in this.handlers)
            {
                if (!qsh.Value.WaitDispose) continue;

                qsh.Value.CfUnLoad();
                qsh.Value.status = SNetEnumHandlerStatus.Unload;


                qsh.Value.CfFree();
                qsh.Value.status = SNetEnumHandlerStatus.Free;
                removeHandlers[qsh.Key] = qsh.Value;
            }
            foreach (var kvdh in removeHandlers)
            {
                this.handlers.Remove(kvdh.Key);
            }
        }

        #region Event

        public event EventHandler<SNetQSecsRcvDataEventArgs> evtReceiveData;
        public void OnReceiveData(SNetQSecsRcvDataEventArgs ea)
        {
            if (this.evtReceiveData == null)
                return;

            this.evtReceiveData(this, ea);
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

        }

        #endregion
    }
}
