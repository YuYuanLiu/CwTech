using CToolkit;
using SensingNet.v0_0.Protocol;
using SensingNet.v0_0.Storage;
using System;
using System.Collections.Generic;
using System.Threading;

namespace SensingNet.v0_0.Signal
{
    public class SignalMgrExecer : IDisposable, IContextFlowRun
    {

        public SignalMgrCfg mgrConfig;
        public String DefaultConfigsFilder = "Config/DeviceConfigs";
        public CToolkit.Config.ConfigCollector<DeviceCfg> configs = new CToolkit.Config.ConfigCollector<DeviceCfg>();
        Dictionary<String, SignalHandler> handlers = new Dictionary<String, SignalHandler>();
        public bool isExec = false;



        ~SignalMgrExecer() { this.Dispose(false); }




        public int CfInit()
        {
            this.mgrConfig = SignalMgrCfg.LoadFromFile();
            this.mgrConfig.SaveToFile();

            return 0;
        }
        public int CfLoad()
        {
            this.isExec = true;
            this.configs.UpdateFromFolder(DefaultConfigsFilder);

            return 0;
        }
        public int CfUnLoad()
        {
            this.isExec = false;
            this.configs.ClearAll();
            this.UpdateHandlerStatus();
            return 0;
        }
        public int CfFree()
        {
            this.isExec = false;
            this.configs.ClearAll();
            this.UpdateHandlerStatus();
            return 0;
        }
        public int CfExec()
        {
            this.configs.UpdateIfOverTime();
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
                catch (Exception ex) { LoggerAssembly.Write(ex); }
            }

            return 0;
        }
        public int CfRunAsyn() { throw new NotImplementedException("此方法不實作重複執行, 請使用CfExec"); }



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
            foreach (var dict in this.configs)
                foreach (var cfg in dict.Value)
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
                        e.handler = hdl;
                        this.OnSignalCapture(e);
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
                    dh.CfUnLoad();
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
