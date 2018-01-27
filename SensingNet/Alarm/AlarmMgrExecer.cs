using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.Alarm
{
    public class AlarmMgrExecer : IDisposable, CToolkit.IContextFlow
    {
        public const String DEFAULT_CONFIGS_FOLDER = "Config/AlarmConfigs/";
        public CToolkit.Config.ConfigCollector<AlarmCfg> configs = new CToolkit.Config.ConfigCollector<AlarmCfg>();
        public Dictionary<String, AlarmHandler> handlers = new Dictionary<String, AlarmHandler>();


        public AlarmMgrExecer() { }



        ~AlarmMgrExecer() { this.Dispose(false); }


        public void DoAlarmCheck(SignalEventArgs ea)
        {
            var tick = DateTime.Now - configs.LastUpdate;
            if (tick.Seconds > 10) { configs.Update(); }//每10秒更新一次Config


            this.RunHandlerStatus();

            foreach (var hdl in this.handlers)
            {
                var cfg = hdl.Value.config;

                var match = false;
                if (ea.DeviceIp == cfg.DeviceIp && ea.DevicePort == cfg.DevicePort)
                    match = true;

                if (!match && ea.DeviceName == cfg.DeviceName)
                    match = true;


                match &= (cfg.DeviceSvid == ea.DeviceSvid);
                if (!match) continue;


                var signals = hdl.Value.ProcessSamples(ea.calibrateData);


                match = signals.Max() > cfg.Max;
                match |= signals.Min() < cfg.Min;

                if (match)
                {
                    var now = DateTime.Now;
                    var dt = hdl.Value.lastAlarmTime;

                    var diff = now - dt;
                    if (diff.TotalSeconds > cfg.AlarmIntervalSec)
                    {
                        hdl.Value.lastAlarmTime = now;
                        this.DoAlarm(new Alarm.AlarmEventArgs() { handler = hdl.Value, signalEventArgs = ea });
                    }
                    else { }
                }

            }



        }


        public int CfInit()
        {
            configs.Load(DEFAULT_CONFIGS_FOLDER);
            return 0;
        }

        public int CfLoad()
        {
            return 0;
        }

        public int CfUnload()
        {
            return 0;
        }

        public int CfFree()
        {
            return 0;
        }





        void RunHandlerStatus()
        {
            if (this.disposed) return;

            //先全部設定為: 等待Dispose
            foreach (var sh in this.handlers)
            {
                sh.Value.WaitDispose = true;
            }


            //Run過所有Config
            //有Config的會解除等待Dispoe
            //有Config的會執行CfRun
            foreach (var cfg in this.configs)
            {
                AlarmHandler ah = null;
                if (!this.handlers.ContainsKey(cfg.Key))
                {
                    ah = new AlarmHandler();
                    this.handlers.Add(cfg.Key, ah);
                }
                else { ah = this.handlers[cfg.Key]; }
                ah.config = cfg.Value;


                //解除等待Dispoe
                ah.WaitDispose = false;

                if (ah.status == EnumHandlerStatus.None)
                {
                    ah.CfInit();
                    ah.status = EnumHandlerStatus.Init;
                }


                if (ah.status == EnumHandlerStatus.Init)
                {
                    ah.CfLoad();
                    ah.status = EnumHandlerStatus.Load;
                }

                //有Config的持續作業
                if (ah.status == EnumHandlerStatus.Load || ah.status == EnumHandlerStatus.Run)
                {
                    ah.status = EnumHandlerStatus.Run;
                }

            }


            //沒有Config的會關閉
            var removeHandlers = new Dictionary<String, AlarmHandler>();
            foreach (var ah in this.handlers)
            {
                if (!ah.Value.WaitDispose) continue;

                ah.Value.CfUnload();
                ah.Value.status = EnumHandlerStatus.Unload;


                ah.Value.CfFree();
                ah.Value.status = EnumHandlerStatus.Free;
                removeHandlers[ah.Key] = ah.Value;
            }
            foreach (var kv in removeHandlers)
            {
                this.handlers.Remove(kv.Key);
            }
        }



        #region Event

        public event EventHandler<AlarmEventArgs> evtAlarm;
        protected void DoAlarm(AlarmEventArgs e)
        {
            if (this.evtAlarm == null) return;
            this.evtAlarm(this, e);
        }



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

        }



        #endregion



    }
}
