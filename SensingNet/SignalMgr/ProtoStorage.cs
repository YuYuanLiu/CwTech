using SensingNet.Storage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SensingNet.Protocol;

namespace SensingNet.SignalMgr
{
    public class ProtoStorage
    {
        public DeviceCfg dCfg;
        Dictionary<Int64, ProtoStorageFile> svidData = new Dictionary<Int64, ProtoStorageFile>();
        Dictionary<Int64, ProtoStorageFile> currSvidPerSecData = new Dictionary<Int64, ProtoStorageFile>();

        ProtoStorageFile GetSvidData(Int64 svid)
        {
            if (!svidData.ContainsKey(svid))
                svidData[svid] = new ProtoStorageFile();
            return svidData[svid];
        }
        ProtoStorageFile GetCurrSvidPerSecData(Int64 svid)
        {
            if (!currSvidPerSecData.ContainsKey(svid))
                currSvidPerSecData[svid] = new ProtoStorageFile();
            return currSvidPerSecData[svid];
        }



        public void Write(ProtoEventArgs ea)
        {
            if (ea.calibrateData.Count <= 0) return;

            var signalCfg = this.dCfg.SignalCfgList.FirstOrDefault(x => x.DeviceSvid == ea.DeviceSvid);
            if (signalCfg == null) return;
            if (String.IsNullOrEmpty(signalCfg.StorageDirectory)) return;


            var now = DateTime.Now;
            {
                var cd = this.GetSvidData(ea.DeviceSvid);
                var fn = string.Format("dt{0}.signal", now.ToString("yyyyMMddHHmm"));
                var dir = System.IO.Path.Combine(signalCfg.StorageDirectory, "dt" + now.ToString("yyyyMMdd"));

                cd.CreateStreamIfNewFile(dir, fn);
                var timestamp = CToolkit.DateTimeStamp.ToTimeStamp(now);
                cd.stream.Write("{0}", timestamp);
                for (int idx = 0; idx < ea.Data.Count; idx++)
                    cd.stream.Write(",{0}", ea.calibrateData[idx]);
                cd.stream.WriteLine();

            }
            {//當前資料, 每秒

                var cd = this.GetCurrSvidPerSecData(ea.DeviceSvid);
                var fn = string.Format("dt{0}.signal.temp", now.ToString("yyyyMMddHHmmss"));
                var dir = System.IO.Path.Combine(signalCfg.StorageDirectory, "current");

                if (cd.CreateStreamIfNewFile(dir, fn))
                {
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


        void DeleteOldCurrent(String dir)
        {
            var datetime = DateTime.Now.AddMinutes(-1);
            var di = new DirectoryInfo(dir);

            //var fn = string.Format("dt{0}.signal", datetime.ToString("yyyyMMddHHmmss"));

            var qf = (from f in di.GetFiles()
                      orderby f.Name
                      select f).ToList();
            for (int idx = 0; idx < qf.Count - 100; idx++)
            {
                qf[idx].Delete();
            }


        }

    }
}
