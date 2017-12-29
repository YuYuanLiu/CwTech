using CToolkit;
using SensingNet.Protocol;
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
        Dictionary<String, ProtoHandler> handlers = new Dictionary<String, ProtoHandler>();
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
            this.RunHandlerStatus();
            return 0;
        }
        public int CfFree()
        {
            this.isExec = false;
            this.configs.Clear();
            this.RunHandlerStatus();
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
            this.RunHandlerStatus();
            return 0;
        }



        void RunHandlerStatus()
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
                ProtoHandler hdl = null;
                if (!this.handlers.ContainsKey(cfg.Key))
                {
                    hdl = new ProtoHandler();
                    this.handlers.Add(cfg.Key, hdl);
                }
                else { hdl = this.handlers[cfg.Key]; }
                hdl.config = cfg.Value;

                //解除等待Dispoe
                hdl.WaitDispose = false;

                if (hdl.status == EnumHandlerStatus.None)
                {
                    hdl.CfInit();
                    hdl.evtCapture += delegate (object sender, ProtoEventArgs e)
                    {
                        if (this.evtCapture == null) return;
                        this.OnCapture(e as SignalEventArgs);
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
            var removeHandlers = new Dictionary<String, ProtoHandler>();
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
        public event EventHandler<SignalEventArgs> evtCapture;
        void OnCapture(SignalEventArgs e)
        {
            if (evtCapture == null) return;
            this.evtCapture(this, e);
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
