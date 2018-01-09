using CToolkit;
using SensingNet.Protocol;
using SensingNet.Storage;
using System;
using System.Collections.Generic;
using System.Threading;

namespace SensingNet.SignalMgr
{
    public class SignalMgrExecer : IDisposable, IContextFlowRun
    {

        public SignalMgrCfg arConfig;
        public const String DEFAULT_DEVICE_CONFIGS_FOLDER = "Config/DeviceConfigs";
        public CToolkit.Config.ConfigCollector<DeviceCfg> configs = new CToolkit.Config.ConfigCollector<DeviceCfg>();
        Dictionary<String, SignalHandler> handlers = new Dictionary<String, SignalHandler>();
        public bool isExec = false;



        ~SignalMgrExecer() { this.Dispose(false); }




        public int CfInit()
        {
            this.arConfig = SignalMgrCfg.LoadFromFile();
            this.arConfig.SaveToFile();

            return 0;
        }
        public int CfLoad()
        {
            this.isExec = true;
            this.configs.Load(DEFAULT_DEVICE_CONFIGS_FOLDER);

            return 0;
        }
        public int CfUnload()
        {
            this.isExec = false;
            this.configs.Clear();
            this.UpdateHandlerStatus();
            return 0;
        }
        public int CfFree()
        {
            this.isExec = false;
            this.configs.Clear();
            this.UpdateHandlerStatus();
            return 0;
        }
        public int CfRun()
        {
            while (this.isExec)
            {
                try
                {
                    this.CfExec();
                    System.Threading.Thread.Sleep(1000);
                }
                catch (Exception ex) { CToolkit.Logging.LoggerDictionary.Singleton.WriteAsyn(ex); }
            }

            return 0;
        }
        public int CfExec()
        {
            this.configs.UpdateIfOverTime();
            this.UpdateHandlerStatus();
            return 0;
        }



        void UpdateHandlerStatus()
        {

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
                SignalHandler hdl = null;
                if (!this.handlers.ContainsKey(cfg.Key))
                {
                    hdl = new SignalHandler();
                    this.handlers.Add(cfg.Key, hdl);
                }
                else { hdl = this.handlers[cfg.Key]; }
                hdl.config = cfg.Value;

                //解除等待Dispoe
                hdl.WaitDispose = false;

                if (hdl.status == EnumHandlerStatus.None)
                {
                    hdl.CfInit();
                    hdl.evtSignalCapture += delegate(object sender, SignalEventArgs e)
                    {
                        //TODO: 應該再往外提, 由使用者決定是否要存成檔案
                        hdl.storager.Write(e);
                        this.OnSignalCapture(e);
                    };
                    hdl.storager.evtCurrentFileChanged += delegate(object sender, FileStorageEventArgs e)
                    {
                        this.OnCurrentFileChanged(new SignalMgrFileStorageEventArgs()
                        {
                            handler = hdl,
                            fileStorageEventArgs = e,
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
                    hdl.CfExec();
                }

            }


            //沒有Config的會關閉Device
            var removeHandlers = new Dictionary<String, SignalHandler>();
            foreach (var kvdh in this.handlers)
            {
                var dh = kvdh.Value;
                if (!dh.WaitDispose) continue;

                if (dh.status == EnumHandlerStatus.Run)
                {
                    dh.CfUnload();
                    dh.status = EnumHandlerStatus.Unload;
                }
                if (dh.status == EnumHandlerStatus.Unload)
                {
                    dh.CfFree();
                    dh.status = EnumHandlerStatus.Free;
                    removeHandlers[kvdh.Key] = kvdh.Value;
                }
            }
            foreach (var kvdh in removeHandlers)
            {
                this.handlers.Remove(kvdh.Key);
            }
        }





        #region Event
        public event EventHandler<SignalEventArgs> evtSignalCapture;
        void OnSignalCapture(SignalEventArgs e)
        {
            if (evtSignalCapture == null) return;
            this.evtSignalCapture(this, e);
        }

        public event EventHandler<SignalMgrFileStorageEventArgs> evtCurrentFileChanged;
        void OnCurrentFileChanged(SignalMgrFileStorageEventArgs e)
        {
            if (evtCurrentFileChanged == null) return;
            this.evtCurrentFileChanged(this, e);
        }

        #endregion

        #region Event Handler



        #endregion

        #region IDisposable
        // Flag: Has Dispose already been called?
        bool disposed = false;

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
                //
                this.DisposeManaged();
            }

            // Free any unmanaged objects here.
            //
            this.DisposeUnmanaged();

            this.DisposeSelf();

            disposed = true;
        }



        void DisposeManaged()
        {
            this.isExec = false;
        }

        void DisposeUnmanaged()
        {

        }

        void DisposeSelf()
        {

        }

        #endregion
    }
}
