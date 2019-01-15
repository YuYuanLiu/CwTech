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
        public CtkPassFilterStruct FilterArgs = new CtkPassFilterStruct()
        {
            CutoffHigh = 512,
            CutoffLow = 5,
            Mode = CtkEnumPassFilterMode.None,
            SampleRate = 1024,
        };




        public void InitFilterIfNull(CtkPassFilterStruct pfargs)
        {

            var flag = this.PassFilter != null;
            flag &= this.FilterArgs.SampleRate == pfargs. SampleRate;
            flag &= this.FilterArgs.CutoffLow == pfargs.CutoffLow;
            flag &= this.FilterArgs.CutoffHigh == pfargs.CutoffHigh;
            if (flag) return;
            this.FilterArgs = pfargs;

            var coff = new double[0];
            switch ( this.FilterArgs.Mode)
            {
                case CtkEnumPassFilterMode.BandPass:
                    coff = FirCoefficients.BandPass(this.FilterArgs.SampleRate, this.FilterArgs.CutoffLow, this.FilterArgs.CutoffHigh);
                    break;

                default:
                    this.PassFilter = null;
                    break;
            }

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
