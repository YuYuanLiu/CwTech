using CToolkit;
using CToolkit.Logging;
using CToolkit.Secs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace SensingNet.SecsMgr
{
    public class SecsMgrExecer : IContextFlowRun, IDisposable
    {
        public SecsMgrCfg config;
        public bool isExec = false;


        public const String DEFAULT_CONFIGS_FOLDER = "Config/QSecsConfigs/";
        public CToolkit.Config.ConfigCollector<QSecsCfg> configs = new CToolkit.Config.ConfigCollector<QSecsCfg>();
        public Dictionary<String, QSecsHandler> handlers = new Dictionary<String, QSecsHandler>();

        ~SecsMgrExecer() { this.Dispose(false); }




        public int CfInit()
        {
            this.config = SecsMgrCfg.LoadFromFile();
            this.config.SaveToFile();

            this.configs.Load(DEFAULT_CONFIGS_FOLDER);


            return 0;
        }
        public int CfLoad()
        {
            return 0;
        }
        public int CfUnload()
        {
            this.configs.Clear();
            this.RunHandlerStatus();
            return 0;
        }
        public int CfFree()
        {
            this.configs.Clear();
            this.RunHandlerStatus();
            this.Dispose(false);
            return 0;
        }
        public int CfRun()
        {
            this.isExec = true;
            while (this.isExec)
            {
                try
                {
                    this.CfExec();
                    System.Threading.Thread.Sleep(1000);
                }
                catch (Exception ex) { LoggerDictionary.Singleton.WriteAsyn(ex); }
            }

            return 0;
        }
        public int CfExec()
        {
            this.configs.UpdateIfOverTime();
            this.RunHandlerStatus();
            return 0;
        }


        /// <summary>
        /// 簡易資料處理的更新
        /// 當收到訊號欲更新各QSecsHandler時, 執行此函式
        /// 想要自行處理資料, 可以忽略
        /// </summary>
        public void DoSecsDataUpdate(SignalEventArgs sea)
        {
            this.configs.UpdateIfOverTime();

            foreach (var qsecscfg in this.configs)
            {
                if (!handlers.ContainsKey(qsecscfg.Key))
                    handlers[qsecscfg.Key] = new QSecsHandler();

                var sh = handlers[qsecscfg.Key];
                sh.cfg = qsecscfg.Value;
                sh.DoRcvSignalData(sea);
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
            foreach (var cfg in this.configs)
            {
                QSecsHandler hdl = null;
                if (!this.handlers.ContainsKey(cfg.Key))
                {
                    hdl = new QSecsHandler();
                    this.handlers.Add(cfg.Key, hdl);
                }
                else { hdl = this.handlers[cfg.Key]; }
                hdl.cfg = cfg.Value;

                //解除等待Dispoe
                hdl.WaitDispose = false;

                if (hdl.status == EnumHandlerStatus.None)
                {
                    hdl.CfInit();
                    hdl.evtReceiveData += delegate (object ss, HsmsConnector_EventArgsRcvData ea)
                    {
                        this.OnReceiveData(new EventArgsSecsRcvData()
                        {
                            handler = ss as QSecsHandler,
                            message = ea.msg
                        });
                    };
                    hdl.status = EnumHandlerStatus.Init;
                }


                if (hdl.status == EnumHandlerStatus.Init)
                {
                    hdl.CfLoad();
                    hdl.status = EnumHandlerStatus.Load;
                }

                //有Config的持續作業
                if (hdl.status == EnumHandlerStatus.Load || hdl.status == EnumHandlerStatus.Run)
                {

                    hdl.status = EnumHandlerStatus.Run;
                    hdl.CfRun();
                }

            }


            //沒有Config的會關閉
            var removeHandlers = new Dictionary<String, QSecsHandler>();
            foreach (var qsh in this.handlers)
            {
                if (!qsh.Value.WaitDispose) continue;

                qsh.Value.CfUnload();
                qsh.Value.status = EnumHandlerStatus.Unload;


                qsh.Value.CfFree();
                qsh.Value.status = EnumHandlerStatus.Free;
                removeHandlers[qsh.Key] = qsh.Value;
            }
            foreach (var kvdh in removeHandlers)
            {
                this.handlers.Remove(kvdh.Key);
            }
        }

        #region Event

        public event EventHandler<EventArgsSecsRcvData> evtReceiveData;
        public void OnReceiveData(EventArgsSecsRcvData ea)
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
