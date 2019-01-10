using CToolkit.v0_1.Numeric;
using MathNet.Filtering.FIR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.Dsp.Block
{
    public class SNetDspBlockFilter_PassFilter
    {

        public OnlineFirFilter PassFilter;
        public int CutoffHigh = 512;
        public int CutoffLow = 5;
        public CtkEnumPassFilterMode Mode = CtkEnumPassFilterMode.None;
        public int SampleRate = 1024;


        public void InitFilterIfNull(CtkEnumPassFilterMode passFilter, int sampleRate, int cutoffLow, int cutoffHigh)
        {

            var flag = this.PassFilter != null;
            flag &= this.SampleRate == sampleRate;
            flag &= this.CutoffLow == cutoffLow;
            flag &= this.CutoffHigh == cutoffHigh;
            if (flag) return;


            this.SampleRate = sampleRate;
            this.CutoffLow = cutoffLow;
            this.CutoffHigh = cutoffHigh;



            var coff = new double[0];
            switch (passFilter)
            {
                case CtkEnumPassFilterMode.BandPass:
                    coff = FirCoefficients.BandPass(sampleRate, cutoffLow, cutoffHigh);
                    break;

                default:
                    this.PassFilter = null;
                    break;
            }

            this.PassFilter = new OnlineFirFilter(coff);
            this.PassFilter = new OnlineFirFilter(coff);



        }



        public double[] ProcessSamples(IEnumerable<double> samples)
        {
            return this.ProcessSamples(samples.ToArray());
        }
        public double[] ProcessSamples(double[] samples)
        {
            if (this.PassFilter == null) return samples;
            return this.PassFilter.ProcessSamples(samples);
        }
    }
}
