using CToolkit;
using CToolkit.v0_1;
using CToolkit.v0_1.Config;
using CToolkit.v0_1.Logging;
using CToolkit.v0_1.Secs;
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
    public class SNetQSecsMgr : ICtkContextFlowRun, IDisposable
    {
        public CtkConfigCollector<SNetQSecsCfg> configs = new CtkConfigCollector<SNetQSecsCfg>();
        public String DefaultConfigsFolder = "Config/QSecsConfigs/";
        public Dictionary<String, SNetQSecsHandler> handlers = new Dictionary<String, SNetQSecsHandler>();
        Task<int> runTask;


        ~SNetQSecsMgr() { this.Dispose(false); }


        #region ICtkContextFlowRun
        public bool CfIsRunning { get; set; }
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
            try { this.OnAfterEachExec(new EventArgs()); }
            catch (Exception ex) { CtkLog.Write(ex, CtkLoggerEnumLevel.Warn); }

            return 0;
        }
        public int CfFree()
        {
            this.configs.ClearAll();
            this.RunHandlerStatus();
            this.Dispose(false);
            return 0;
        }
        public int CfInit()
        {
            this.configs.UpdateFromFolder(DefaultConfigsFolder);


            return 0;
        }
        public int CfLoad()
        {
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
        #endregion 

        void RunHandlerStatus()
        {
            if (this.disposed) return;

            //先全部設定為: 等待Dispose
            foreach (var hdl in this.handlers)
            {
                hdl.Value.IsWaitDispose = true;
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
                    hdl.IsWaitDispose = false;

                    if (hdl.status == SNetEnumHandlerStatus.None)
                    {
                        hdl.CfInit();
                        hdl.evtReceiveData += delegate (object ss, CtkHsmsConnectorRcvDataEventArg ea)
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
                if (!qsh.Value.IsWaitDispose) continue;

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
            if (this.evtReceiveData == null) return;
            this.evtReceiveData(this, ea);
        }

        public event EventHandler evtAfterEachExec;
        public void OnAfterEachExec(EventArgs ea)
        {
            if (this.evtAfterEachExec == null) return;
            this.evtAfterEachExec(this, ea);
        }


        #endregion

        #region Dispose

        protected bool disposed = false;
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void DisposeManaged()
        {

        }
        protected virtual void DisposeSelf()
        {

        }
        protected virtual void DisposeUnManaged()
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
