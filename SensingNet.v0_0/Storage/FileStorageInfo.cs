using CToolkit.v0_1.NumericProc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SensingNet.v0_0.Storage
{
    public class FileStorageInfo
    {
        public FileStorageFormat header = new FileStorageFormat_Csv0_1();
        public SignalCollector collector = new SignalCollector();



        public void WriteHeader(StreamWriter sw)
        {
            sw.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(this.header));
        }

        /// <summary>
        /// if utc.Kind is Unspecified then as UTC
        /// </summary>
        /// <param name="datetime"></param>
        /// <param name="values"></param>
        public void WriteValues(StreamWriter sw, DateTime datetime, IEnumerable<double> values)
        {
            this.header.WriteValues(sw, datetime, values);
        }


        public void ReadStream(StreamReader sr)
        {
            var headerStr = sr.ReadLine();
            if (String.IsNullOrEmpty(headerStr)) return;
            var header = Newtonsoft.Json.JsonConvert.DeserializeObject<FileStorageFormat>(headerStr);

            if (header.FormatName == typeof(FileStorageFormat_Csv_0).Name) 
                    this.header = Newtonsoft.Json.JsonConvert.DeserializeObject<FileStorageFormat_Csv_0>(headerStr);
            else if (header.FormatName == typeof(FileStorageFormat_Csv0_0).Name)
                this.header = Newtonsoft.Json.JsonConvert.DeserializeObject<FileStorageFormat_Csv0_0>(headerStr);
            else if (header.FormatName == typeof(FileStorageFormat_Csv0_1).Name)
                this.header = Newtonsoft.Json.JsonConvert.DeserializeObject<FileStorageFormat_Csv0_1>(headerStr);


            this.header.ReadStream(sr, this.collector);

        }




        public void ReadStream(StreamReader sr, CToolkit.v0_1.NumericProc.EnumPassFilter passFilter, int sampleRate, int cutoffLow, int cutoffHigh)
        {
            var filter = new FftOnlineFilter();
            filter.SetFilter(passFilter, sampleRate, cutoffLow, cutoffHigh);




        }





    }
}
