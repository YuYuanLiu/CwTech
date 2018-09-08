using CToolkit.NumericProc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SensingNet.v0_0.Storage
{
    public class FileStorageInfo
    {
        public FileStorageFormat header = new FileStorageFormat_Csv0_0();
        public SignalCollector collector = new SignalCollector();



        public void WriteHeader(StreamWriter sw)
        {
            sw.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(this.header));
        }

        /// <summary>
        /// if utc.Kind is Unspecified then as UTC
        /// </summary>
        /// <param name="utc"></param>
        /// <param name="values"></param>
        public void WriteValues(StreamWriter sw, DateTime utc, IEnumerable<double> values)
        {
            //if utc.Kind is Unspecified then as UTC
            if (utc.Kind == DateTimeKind.Unspecified)
                utc = DateTime.SpecifyKind(utc, DateTimeKind.Utc);

            //Loca / Utc ToUtcTimestamp 皆會轉成 UTC
            var utcTimestamp = CToolkit.DateTimeStamp.ToUtcTimestamp(utc);
            var localDt = utc.ToLocalTime();

            sw.Write("{0}", utcTimestamp);
            foreach (var val in values)
                sw.Write(",{0}", val);
            sw.WriteLine();

        }


        public void ReadStream(StreamReader sr)
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

                //來源時間為Universal (檔案儲存時間)
                var dt = CToolkit.DateTimeStamp.ToLocalDateTimeFromTimestamp(timestamp);

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




        public void ReadStream(StreamReader sr, CToolkit.NumericProc.EnumPassFilter passFilter, int sampleRate, int cutoffLow, int cutoffHigh)
        {
            var filter = new FftOnlineFilter();
            filter.SetFilter(passFilter, sampleRate, cutoffLow, cutoffHigh);




        }





    }
}
