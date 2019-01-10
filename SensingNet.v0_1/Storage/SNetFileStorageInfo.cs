using CToolkit.v0_1.Numeric;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SensingNet.v0_1.Storage
{
    public class SNetFileStorageInfo
    {
        public SNetFileStorageFormat fsFormat = new SNetFileStorageFormat_Csv0_2();
        public SNetSignalCollector collector = new SNetSignalCollector();



        public void WriteHeader(StreamWriter sw)
        {
            sw.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(this.fsFormat));
        }

        /// <summary>
        /// if utc.Kind is Unspecified then as UTC
        /// </summary>
        /// <param name="datetime"></param>
        /// <param name="values"></param>
        public void WriteValues(StreamWriter sw, DateTime datetime, IEnumerable<double> values)
        {
            this.fsFormat.WriteValues(sw, datetime, values);
        }


        public void ReadStream(StreamReader sr)
        {
            var headerStr = sr.ReadLine();
            if (String.IsNullOrEmpty(headerStr)) return;
            var header = Newtonsoft.Json.JsonConvert.DeserializeObject<SNetFileStorageFormat>(headerStr);

            if (header.FormatName == typeof(FileStorageFormat_Csv0_0).Name)
                this.fsFormat = Newtonsoft.Json.JsonConvert.DeserializeObject<FileStorageFormat_Csv0_0>(headerStr);
            else if (header.FormatName == typeof(FileStorageFormat_Csv0_1).Name)
                this.fsFormat = Newtonsoft.Json.JsonConvert.DeserializeObject<FileStorageFormat_Csv0_1>(headerStr);
            else if (header.FormatName == typeof(SNetFileStorageFormat_Csv0_2).Name)
                this.fsFormat = Newtonsoft.Json.JsonConvert.DeserializeObject<SNetFileStorageFormat_Csv0_2>(headerStr);


            this.fsFormat.ReadStream(sr, this.collector);

        }




        public void ReadStream(StreamReader sr, CtkEnumPassFilterMode passFilter, int sampleRate, int cutoffLow, int cutoffHigh)
        {
            var filter = new CtkFftOnlineFilter();
            filter.SetFilter(passFilter, sampleRate, cutoffLow, cutoffHigh);

            //TODO: й|е╝з╣жи
        }





    }
}
