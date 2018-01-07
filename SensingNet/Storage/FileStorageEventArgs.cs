using System;
using System.IO;
using System.Threading;

namespace SensingNet.Storage
{
    public class FileStorageEventArgs : EventArgs
    {

        public String pprevFile;
        public String prevFile;
        public String currFile;
        public String directory;
        public System.IO.StreamWriter stream;

        public bool CreateStreamIfNewFile(String dirPath, String fn)
        {


            if (Monitor.TryEnter(this, new TimeSpan(0, 0, 10)))
            {
                try
                {
                    if (this.currFile == null || this.currFile != fn)
                    {
                        this.directory = dirPath;
                        this.pprevFile = this.prevFile;
                        this.prevFile = this.currFile;
                        this.currFile = fn;


                        if (this.stream != null)
                        {
                            this.stream.Flush();
                            this.stream.Close();
                            this.stream.Dispose();
                            this.stream = null;
                        }

                        var fi = GetCurrFileInfo();
                        if (!fi.Directory.Exists)
                            fi.Directory.Create();

                        this.stream = new StreamWriter(fi.FullName);
                        this.stream.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(new Storage.StorageInfoHeader()));

                        return true;
                    }
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }
            return false;
        }

        public bool CloseStreamIfNewFile(String dirPath, String fn)
        {

            if (Monitor.TryEnter(this, new TimeSpan(0, 0, 10)))
            {
                try
                {
                    if (this.currFile == null || this.currFile != fn)
                    {
                        if (this.stream != null)
                        {
                            this.stream.Flush();
                            this.stream.Close();
                            this.stream.Dispose();
                            this.stream = null;
                        }
                        return true;
                    }
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }
            return false;
        }


        public FileInfo GetPPrevFileInfo() { return String.IsNullOrEmpty(this.pprevFile) ? null : new FileInfo(Path.Combine(this.directory, this.pprevFile)); }
        public FileInfo GetPrevFileInfo() { return String.IsNullOrEmpty(this.prevFile) ? null : new FileInfo(Path.Combine(this.directory, this.prevFile)); }
        public FileInfo GetCurrFileInfo() { return String.IsNullOrEmpty(this.currFile) ? null : new FileInfo(Path.Combine(this.directory, this.currFile)); }









    }
}
