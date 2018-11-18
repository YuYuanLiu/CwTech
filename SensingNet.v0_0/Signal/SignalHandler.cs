using SensingNet.v0_0.Protocol;
using SensingNet.v0_0.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace SensingNet.v0_0.Signal
{
    public class SignalHandler : CToolkit.IContextFlowRun
    {
        public DeviceCfg config;
        public ProtoEthernet etherneter;
        public EnumHandlerStatus status = EnumHandlerStatus.None;
        public bool WaitDispose = false;



        void capturer_evtDataRcv(object sender, EventArgs ea)
        {
            var eaCapture = ea as SignalEventArgs;
            eaCapture.calibrateData = new List<double>();

            var SignalCfg = this.config.SignalCfgList.FirstOrDefault(x => x.DeviceSvid == eaCapture.DeviceSvid);
            if (SignalCfg == null) return;

            for (int idx = 0; idx < eaCapture.Data.Count; idx++)
            {
                var signal = eaCapture.Data[idx];
                //var signal = d / (Math.Pow(2, 23) - 1) * 5; //轉回電壓
                signal = signal * SignalCfg.CalibrateSysScale + SignalCfg.CalibrateSysOffset;//轉成System值
                eaCapture.calibrateData.Add(signal * SignalCfg.CalibrateUserScale + SignalCfg.CalibrateUserOffset);//轉入User Define
            }


            eaCapture.DeviceName = this.config.DeviceName;
            eaCapture.DeviceIp = this.config.RemoteIp;
            eaCapture.DevicePort = this.config.RemotePort;
            eaCapture.RcvDateTime = DateTime.Now;
            this.OnSignalCapture(eaCapture);
        }



        public int CfInit()
        {
            if (this.config == null) throw new SensingNetException("沒有設定參數");



            var localIpAddr = CToolkit.CtkUtil.GetLikelyFirst127Ip(this.config.LocalIp, this.config.RemoteIp);
            var localEndPoint = new IPEndPoint(localIpAddr, this.config.LocalPort);
            var remoteEndPoint = new IPEndPoint(IPAddress.Parse(this.config.RemoteIp), this.config.RemotePort);


            this.etherneter = new ProtoEthernet(
                localEndPoint,
                remoteEndPoint,
                this.config);
            this.etherneter.evtDataReceive += capturer_evtDataRcv;

            return 0;
        }
        public int CfLoad()
        {

            return 0;
        }
        public int CfUnLoad()
        {
            this.etherneter.Disconnect();
            //this.evtCapture = null;
            return 0;
        }
        public int CfFree()
        {
            return 0;

        }
        public int CfRun()
        {
            throw new NotImplementedException("此方法不實作重複執行, 請使用CfExec");
        }
        public int CfExec()
        {

            this.etherneter.ConnectIfNo();

            return 0;
        }











        #region Event
        public event EventHandler<SignalEventArgs> evtSignalCapture;
        void OnSignalCapture(SignalEventArgs e)
        {
            if (evtSignalCapture == null) return;
            this.evtSignalCapture(this, e);
        }
        #endregion

    }
}
