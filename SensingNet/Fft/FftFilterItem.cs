using CToolkit.NumericProc;
using MathNet.Filtering.FIR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensingNet.Fft
{
    public class FftFilterItem
    {
        OnlineFirFilter filter;

        public EnumPassFilter passFilter { get; private set; }
        public int sampleRate { get; private set; }
        public int cutoffLow { get; private set; }
        public int cutoffHigh { get; private set; }

        public OnlineFirFilter SetFilter(EnumPassFilter passFilter, int sampleRate, int cutoffLow, int cutoffHigh)
        {
            var flag = this.filter != null;
            flag &= this.passFilter == passFilter;
            flag &= this.sampleRate == sampleRate;
            flag &= this.cutoffLow == cutoffLow;
            flag &= this.cutoffHigh == cutoffHigh;

            if (flag) return filter;


            this.passFilter = passFilter;
            this.sampleRate = sampleRate;
            this.cutoffLow = cutoffLow;
            this.cutoffHigh = cutoffHigh;


            var coff = new double[0];
            switch (passFilter)
            {
                case EnumPassFilter.BandPass:
                    coff = FirCoefficients.BandPass(sampleRate, cutoffLow, cutoffHigh);
                    break;
                case EnumPassFilter.LowPass:
                    coff = FirCoefficients.LowPass(sampleRate, cutoffLow);
                    break;
                case EnumPassFilter.HighPass:
                    coff = FirCoefficients.HighPass(sampleRate, cutoffHigh);
                    break;
                default:
                    throw new NotImplementedException();
            }

            filter = new OnlineFirFilter(coff);
            return filter;
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

        public double[] ProcessSamplesInterpolation(double[] samples)
        {
            samples = NpUtil.Interpolation(samples, this.sampleRate);
            if (this.filter == null) return samples;
            return this.filter.ProcessSamples(samples);
        }

    }
}
