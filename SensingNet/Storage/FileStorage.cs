using SensingNet.Storage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SensingNet.Protocol;
using SensingNet.SignalMgr;

namespace SensingNet.Storage
{
    public class FileStorage
    {
        public DeviceCfg dCfg;
        Dictionary<Int64, FileStorageEventArgs> svidData = new Dictionary<Int64, FileStorageEventArgs>();
        Dictionary<Int64, FileStorageEventArgs> currSvidPerSecData = new Dictionary<Int64, FileStorageEventArgs>();



        FileStorageEventArgs GetSvidData(Int64 svid)
        {
            if (!svidData.ContainsKey(svid))
                svidData[svid] = new FileStorageEventArgs();
            return svidData[svid];
        }
        FileStorageEventArgs GetCurrSvidPerSecData(Int64 svid)
        {
            if (!currSvidPerSecData.ContainsKey(svid))
                currSvidPerSecData[svid] = new FileStorageEventArgs();
            return currSvidPerSecData[svid];
        }



        public void Write(SignalEventArgs ea)
        {
            if (ea.calibrateData.Count <= 0) return;

            var signalCfg = this.dCfg.SignalCfgList.FirstOrDefault(x => x.DeviceSvid == ea.DeviceSvid);
            if (signalCfg == null) return;
            if (String.IsNullOrEmpty(signalCfg.StorageDirectory)) return;


            var now = DateTime.Now;
            {//每分鐘 -> 實際儲存
                var cd = this.GetSvidData(ea.DeviceSvid);
                var fn = string.Format("dt{0}.signal", now.ToString("yyyyMMddHHmm"));
                var dir = System.IO.Path.Combine(signalCfg.StorageDirectory, "dt" + now.ToString("yyyyMMdd"));

                cd.CreateStreamIfNewFile(dir, fn);
                var timestamp = CToolkit.DateTimeStamp.ToTimeStamp(now);
                cd.stream.Write("{0}", timestamp);
                for (int idx = 0; idx < ea.Data.Count; idx++)
                    cd.stream.Write(",{0}", ea.calibrateData[idx]);
                cd.stream.WriteLine();


                DeleteOld(signalCfg, dir);

            }
            {//每秒鐘 -> 暫時儲存, 當前資料

                var cd = this.GetCurrSvidPerSecData(ea.DeviceSvid);
                var fn = string.Format("dt{0}.signal.temp", now.ToString("yyyyMMddHHmmss"));
                var dir = System.IO.Path.Combine(signalCfg.StorageDirectory, "current");

                if (cd.CreateStreamIfNewFile(dir, fn))
                {

                    this.OnCurrentFileChanged(cd);

                    var prevfi = cd.GetPrevFileInfo();
                    if (prevfi != null)
                    {
                        var copyto = new FileInfo(prevfi.FullName.Substring(0, prevfi.FullName.Length - 5)); // Remove .temp

                        if (prevfi.Exists
                            && prevfi.Name.Contains(".temp")
                            && !copyto.Exists
                            )
                            prevfi.CopyTo(copyto.FullName);
                    }
                    DeleteOldCurrent(dir);

                }
                var timestamp = CToolkit.DateTimeStamp.ToTimeStamp(now);
                cd.stream.Write("{0}", timestamp);
                for (int idx = 0; idx < ea.Data.Count; idx++)
                    cd.stream.Write(",{0}", ea.calibrateData[idx]);
                cd.stream.WriteLine();
            }

        }


        public void UpdateCurrStorageFile()
        {
            var now = DateTime.Now;
            var fn = string.Format("dt{0}.signal.temp", now.ToString("yyyyMMddHHmmss"));
            var newfi = new FileInfo(fn);

            foreach (var kv in this.currSvidPerSecData)
            {
                try
                {
                    var val = kv.Value;
                    var signalCfg = this.dCfg.SignalCfgList.FirstOrDefault(x => x.DeviceSvid == kv.Key);
                    if (signalCfg == null) continue;

                    var cd = this.GetCurrSvidPerSecData(signalCfg.DeviceSvid);
                    var dir = System.IO.Path.Combine(signalCfg.StorageDirectory, "current");
                    if (cd.CloseStreamIfNewFile(dir, fn))
                    {
                        this.OnCurrentFileChanged(cd);

                        var currfi = val.GetCurrFileInfo();
                        var copyto = new FileInfo(currfi.FullName.Substring(0, currfi.FullName.Length - 5)); // Remove .temp

                        if (currfi.Exists
                            && currfi.Name.Contains(".temp")
                            && !copyto.Exists
                            )
                            currfi.CopyTo(copyto.FullName);
                    }
                }
                catch (Exception ex) { CToolkit.Logging.LoggerDictionary.Singleton.WriteAsyn(ex); }
            }
        }

        void DeleteOld(SignalCfg signalCfg, String dir)
        {
            var now = DateTime.Now;
            var datetime = now.AddSeconds(-signalCfg.PurgeTimestamp);
            var di = new DirectoryInfo(dir);


            var qf = (from d in di.GetDirectories()
                      orderby d.Name
                      select d).ToList();
            for (int idx = 0; idx < qf.Count - 100; idx++)
            {
                qf[idx].Delete();
            }

            foreach (var d in di.GetDirectories())
            {
                if (string.Compare(d.Name, "dt" + datetime.ToString("yyyyMMdd"), true) < 0)
                    d.Delete(true);
            }


        }

        void DeleteOldCurrent(String dir)
        {
            //var datetime = DateTime.Now.AddMinutes(-1);
            //var fn = string.Format("dt{0}.signal", datetime.ToString("yyyyMMddHHmmss"));
            var di = new DirectoryInfo(dir);
            var qf = (from f in di.GetFiles()
                      orderby f.Name
                      select f).ToList();
            for (int idx = 0; idx < qf.Count - 100; idx++)
            {
                qf[idx].Delete();
            }
        }



        #region Event

        public event EventHandler<FileStorageEventArgs> evtCurrentFileChanged;
        public void OnCurrentFileChanged(FileStorageEventArgs ea)
        {
            if (evtCurrentFileChanged == null) return;
            this.evtCurrentFileChanged(this, ea);
        }


        #endregion



    }
}
