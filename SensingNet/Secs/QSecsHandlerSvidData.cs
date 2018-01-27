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



        public void InitFilterIfNull(EnumPassFilter passFilter, int sampleRate, int cutoffLow, int cutoffHigh)
        {
            switch (passFilter)
            {
                case EnumPassFilter.BandPass:
                    var coff = MathNet.Filtering.FIR.FirCoefficients.BandPass(
                        sampleRate,
                        cutoffLow,
                        cutoffHigh
                    );

                    filter = new MathNet.Filtering.FIR.OnlineFirFilter(coff);
                    break;

                default:
                    filter = null;
                    break;
            }

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
