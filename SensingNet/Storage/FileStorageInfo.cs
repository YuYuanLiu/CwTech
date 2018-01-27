using Cudafy.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.Storage
{
    public class FileStorageInfo
    {
        public FileStorageHeader header = new FileStorageHeader();
        public SignalCollector collector = new SignalCollector();

        




        public void ReadStream(System.IO.StreamReader sr)
        {
            var headerStr = sr.ReadLine();
            if (String.IsNullOrEmpty(headerStr)) return;
            var header = Newtonsoft.Json.JsonConvert.DeserializeObject<FileStorageInfo>(headerStr);


            SignalPerSec tfbps = null;
            for (var line = sr.ReadLine(); line != null; line = sr.ReadLine())
            {
                //切割資料
                var vals = line.Split(',');
                if (vals.Length < 2) continue;

                //第一筆為 timestamp
                var timestamp = 0.0;
                if (!double.TryParse(vals[0], out timestamp)) continue;
                //來源時間為Universal
                var dt = CToolkit.DateTimeStamp.ToLocalDateTimeFromTimeStamp(timestamp);

                if (tfbps == null)
                {
                    tfbps = new SignalPerSec();
                    tfbps.dt = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                }
                else if ((dt - tfbps.dt).TotalSeconds >= 1.0)
                {//若時間變更超過一秒, 就加一個物件來儲存

                    this.collector.AddLast(tfbps);
                    tfbps = new SignalPerSec();
                    tfbps.dt = dt;
                }

                for (int idx = 1; idx < vals.Length; idx++)
                {
                    var data = 0.0;
                    if (!double.TryParse(vals[idx], out data)) continue;
                    tfbps.signals.Add(data);
                }

            }
            if (tfbps.signals.Count > 0 && this.collector.LastOrDefault() != tfbps)
                this.collector.AddLast(tfbps);





        }


        public void WriteStream()
        {

        }




    }
}
