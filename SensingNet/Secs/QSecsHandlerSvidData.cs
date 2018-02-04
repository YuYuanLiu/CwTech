using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.Secs
{
    /// <summary>
    /// 提供 QSecsHandler 簡易資料處理
    /// 包含 資料收集, 濾波
    /// </summary>
    public class QSecsHandlerSvidData
    {
        public Storage.SignalCollector SignalCollector = new Storage.SignalCollector();
        public MathNet.Filtering.FIR.OnlineFirFilter filter;

        int sampleRate;
        int cutoffLow;
        int cutoffHigh;


        public void InitFilterIfNull(EnumPassFilter passFilter, int sampleRate, int cutoffLow, int cutoffHigh)
        {

            var flag = this.filter != null;
            flag &= this.sampleRate == sampleRate;
            flag &= this.cutoffLow == cutoffLow;
            flag &= this.cutoffHigh == cutoffHigh;
            if (flag) return;


            this.sampleRate = sampleRate;
            this.cutoffLow = cutoffLow;
            this.cutoffHigh = cutoffHigh;



            var coff = new double[0];
            switch (passFilter)
            {
                case EnumPassFilter.BandPass:
                    coff = MathNet.Filtering.FIR.FirCoefficients.BandPass(
                       sampleRate,
                       cutoffLow,
                       cutoffHigh
                    );

                    break;

                default:
                    filter = null;
                    break;
            }

            filter = new MathNet.Filtering.FIR.OnlineFirFilter(coff);



        }



        public double[] ProcessSamples(IEnumerable<double> samples)
        {
            return this.ProcessSamples(samples.ToArray());
        }
        public double[] ProcessSamples(double[] samples)
        {
            if (this.filter == null) return samples;
            return this.filter.ProcessSamples(samples);
        }

    }
}
