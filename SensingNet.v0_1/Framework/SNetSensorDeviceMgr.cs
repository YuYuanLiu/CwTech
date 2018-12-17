using CToolkit;
using SensingNet.v0_1.Device;
using SensingNet.v0_1.Signal;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SensingNet.v0_1.Framework
{
    public class SNetSensorDeviceMgr : IDisposable, IContextFlowRun
    {

        public String DefaultConfigsFilder = "Config/DeviceConfigs";
        public CToolkit.Config.ConfigCollector<SNetSensorDeviceCfg> configs = new CToolkit.Config.ConfigCollector<SNetSensorDeviceCfg>();
        Dictionary<String, SNetSensorDeviceHandler> handlers = new Dictionary<String, SNetSensorDeviceHandler>();
        Task<int> runTask;



        ~SNetSensorDeviceMgr() { this.Dispose(false); }


        public bool CfIsRunning { get; set; }
        public int CfInit()
        {

            return 0;
        }
        public int CfLoad()
        {
            this.CfIsRunning = true;
            this.configs.UpdateFromFolder(DefaultConfigsFilder);

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
            this.CfIsRunning = true;
            while (!this.disposed && this.CfIsRunning)
            {
                try
                {
                    this.CfExec();
                    System.Threading.Thread.Sleep(1000);
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
            this.CfIsRunning = false;
            this.configs.ClearAll();
            this.UpdateHandlerStatus();
            return 0;
        }
        public int CfFree()
        {
            this.CfIsRunning = false;
            this.configs.ClearAll();
            this.UpdateHandlerStatus();
            return 0;
        }



        void UpdateHandlerStatus()
        {
            //是否存在, 不需dispose
            var dict = new Dictionary<string, bool>();

            //先全部設定為: 等待Dispose
            foreach (var kvhdl in this.handlers)
                dict[kvhdl.Key] = false;


            //Run過所有Config
            //有Config的會解除等待Dispoe
            //有Config的會執行CfRun
            foreach (var dictcfg in this.configs)
                foreach (var kvcfg in dictcfg.Value)
                {
                    SNetSensorDeviceHandler hdl = null;
                    if (!this.handlers.ContainsKey(kvcfg.Key))
                    {
                        hdl = new SNetSensorDeviceHandler();
                        this.handlers.Add(kvcfg.Key, hdl);
                    }
                    else { hdl = this.handlers[kvcfg.Key]; }
                    hdl.Config = kvcfg.Value;

                    //解除等待Dispoe
                    dict[kvcfg.Key] = true;

                    if (hdl.Status == SNetEnumHandlerStatus.None)
                    {
                        hdl.CfInit();
                        hdl.evtSignalCapture += delegate (object sender, SNetSignalEventArgs e)
                        {
                            e.Sender = hdl;
                            this.OnSignalCapture(e);
                        };

                        hdl.Status = SNetEnumHandlerStatus.Init;
                    }
                    if (hdl.Status == SNetEnumHandlerStatus.Init)
                    {
                        hdl.CfLoad();
                        hdl.Status = SNetEnumHandlerStatus.Load;
                    }

                    //有Config的持續作業
                    if (hdl.Status == SNetEnumHandlerStatus.Load || hdl.Status == SNetEnumHandlerStatus.Run)
                    {
                        hdl.Status = SNetEnumHandlerStatus.Run;
                        if (!hdl.CfIsRunning)
                            hdl.CfRunAsyn();
                    }

                }


            //沒有Config的會關閉Device
            foreach (var kv in dict)
            {
                if (kv.Value) continue;
                var hdl = this.handlers[kv.Key];

                if (hdl.Status == SNetEnumHandlerStatus.Run)
                {
                    hdl.CfUnLoad();
                    hdl.Status = SNetEnumHandlerStatus.Unload;
                }
                if (hdl.Status == SNetEnumHandlerStatus.Unload)
                {
                    hdl.CfFree();
                    hdl.Status = SNetEnumHandlerStatus.Free;
                }
                if (hdl.Status == SNetEnumHandlerStatus.Free)
                {
                    this.handlers.Remove(kv.Key);
                }
            }


        }





        #region Event
        public event EventHandler<SNetSignalEventArgs> evtSignalCapture;
        void OnSignalCapture(SNetSignalEventArgs e)
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
        }

        void DisposeUnmanaged()
        {

        }

        void DisposeSelf()
        {
            this.CfIsRunning = false;
        }

        #endregion
    }
}
