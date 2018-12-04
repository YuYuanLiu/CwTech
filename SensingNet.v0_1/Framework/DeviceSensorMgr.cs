using CToolkit;
using SensingNet.v0_1.Device;
using SensingNet.v0_1.Signal;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SensingNet.v0_1.Framework
{
    public class DeviceSensorMgr : IDisposable, IContextFlowRun
    {

        public String DefaultConfigsFilder = "Config/DeviceConfigs";
        public CToolkit.Config.ConfigCollector<SensorDeviceCfg> configs = new CToolkit.Config.ConfigCollector<SensorDeviceCfg>();
        Dictionary<String, SensorDeviceHandler> handlers = new Dictionary<String, SensorDeviceHandler>();
        Task<int> runTask;



        ~DeviceSensorMgr() { this.Dispose(false); }



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
                    SensorDeviceHandler hdl = null;
                    if (!this.handlers.ContainsKey(kvcfg.Key))
                    {
                        hdl = new SensorDeviceHandler();
                        this.handlers.Add(kvcfg.Key, hdl);
                    }
                    else { hdl = this.handlers[kvcfg.Key]; }
                    hdl.Config = kvcfg.Value;

                    //解除等待Dispoe
                    dict[kvcfg.Key] = true;

                    if (hdl.status == EnumHandlerStatus.None)
                    {
                        hdl.CfInit();
                        hdl.evtSignalCapture += delegate (object sender, SignalEventArgs e)
                        {
                            e.Sender = hdl;
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
                        if (!hdl.CfIsRunning)
                            hdl.CfRunAsyn();
                    }

                }


            //沒有Config的會關閉Device
            foreach (var kv in dict)
            {
                if (kv.Value) continue;
                var hdl = this.handlers[kv.Key];

                if (hdl.status == EnumHandlerStatus.Run)
                {
                    hdl.CfUnLoad();
                    hdl.status = EnumHandlerStatus.Unload;
                }
                if (hdl.status == EnumHandlerStatus.Unload)
                {
                    hdl.CfFree();
                    hdl.status = EnumHandlerStatus.Free;
                }
                if (hdl.status == EnumHandlerStatus.Free)
                {
                    this.handlers.Remove(kv.Key);
                }
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
