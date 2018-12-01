using CToolkit;
using SensingNet.v0_1.Signal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SensingNet.v0_1.Storage
{
    public class FileStorage : IDisposable
    {
        public FileStorageCfg Config = new FileStorageCfg();

        FileInfo fi;
        StreamWriter fwriter;
        FileStorageInfo fsInfo = new FileStorageInfo();

        public FileStorage(string dir) { this.Config.DirectoryPath = dir; }

        public void Write(SignalEventArgs ea)
        {
            if (ea.CalibrateData.Count <= 0) return;
            var now = DateTime.Now;
            var nowUtc = now.ToUniversalTime();

            //每分鐘 -> 實際儲存
            var fn = string.Format("dt{0}.signal.lock", now.ToString("yyyyMMddHHmm"));
            var dir = Path.Combine(this.Config.DirectoryPath, "dt" + now.ToString("yyyyMMdd"));
            var currfp = Path.Combine(dir, fn);
            var currfi = new FileInfo(currfp);


            try
            {
                //一秒內要進入
                if (!Monitor.TryEnter(this, 1000))
                {
                    CtkLog.WarnNs(this, "時限內無法進入Signal file create");
                    return;
                }
                //檔名是否需要置換了
                if (this.fwriter == null || this.fi.FullName != currfi.FullName)
                {
                    var lockFp = this.fi.FullName;
                    var nonLockFp = Regex.Replace(lockFp, @"\.lcok$", "");
                    if (this.fwriter != null)
                    {
                        this.CloseStream(ref this.fwriter);
                        File.Move(lockFp, nonLockFp);
                    }


                    //不等待Event的工作, 避免來不及寫入
                    Task.Factory.StartNew(() =>
                    {
                        this.OnFileChanged(new FileStorageEventArgs()
                        {
                            PrevFilePath = nonLockFp,
                            CurrFilePath = currfi.FullName,
                        });
                    });

                    this.fi = currfi;
                    this.fwriter = new StreamWriter(currfi.FullName);//操作用 lock 檔
                    this.fsInfo.WriteHeader(this.fwriter);
                }
                //檔案是當前時區
                this.fsInfo.WriteValues(this.fwriter, nowUtc, ea.CalibrateData);
            }
            finally { Monitor.Exit(this); }


        }




        void CloseStream(ref StreamWriter sw)
        {
            if (sw == null) return;
            using (sw)
                sw.Close();
            sw = null;
        }

        void DeleteOld()
        {
            if (this.Config.PurgeIntervalSecond == 0) return;//沒設定不進行Purge

            var now = DateTime.Now;
            var purgeDt = now.AddSeconds(-this.Config.PurgeIntervalSecond);
            var roobtDi = new DirectoryInfo(this.Config.DirectoryPath);

            var yyyymmdd = purgeDt.ToString("yyyyMMdd");
            var yyyymmddhhmm = purgeDt.ToString("yyyyMMddHHmm");

            //找出超過時間的目錄, 直接刪除目錄
            var diList = (from row in roobtDi.GetDirectories()
                          where string.Compare(row.Name, "dt" + yyyymmdd) < 0
                          select row).ToList();
            foreach (var di in diList)
                di.Delete(true);



            //找出Purge那天的目錄
            var purgeDayDi = (from row in roobtDi.GetDirectories()
                              where string.Compare(row.Name, "dt" + yyyymmdd) == 0
                              select row).FirstOrDefault();
            if (purgeDayDi == null) return;

            //找出Purge那天, 需要被Purge的檔案
            var fiList = (from row in roobtDi.GetFiles()
                          where string.Compare(row.Name, "dt" + yyyymmddhhmm + ".signal") < 0
                          select row).ToList();
            foreach (var fi in fiList)
                fi.Delete();


        }





        #region Event

        public event EventHandler<FileStorageEventArgs> evtFileChanged;
        public void OnFileChanged(FileStorageEventArgs ea)
        {
            if (evtFileChanged == null) return;
            this.evtFileChanged(this, ea);
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
            this.CloseStream(ref this.fwriter);

            EventUtil.RemoveEventHandlersFrom(delegate (Delegate dlgt) { return true; }, this);
        }

        #endregion


    }
}
