using CToolkit;
using CToolkit.Secs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;

namespace SensingNet.Alarm
{
    public class AlarmHandler : IContextFlow, IDisposable
    {
        public AlarmCfg config;
        public DateTime lastAlarmTime;
        public EnumHandlerStatus status = EnumHandlerStatus.None;
        public bool WaitDispose;

        public MathNet.Filtering.FIR.OnlineFirFilter filter;




        public int CfInit()
        {
            switch (this.config.PassFilter)
            {
                case EnumPassFilter.BandPass:
                    var coff = MathNet.Filtering.FIR.FirCoefficients.BandPass(
                        this.config.PassFilter_SampleRate,
                        this.config.PassFilter_CutoffLow,
                        this.config.PassFilter_CutoffHigh
                    );

                    filter = new MathNet.Filtering.FIR.OnlineFirFilter(coff);
                    break;

                default:
                    filter = null;
                    break;
            }



            return 0;
        }

        public int CfLoad()
        {


            return 0;
        }

        public int CfUnload()
        {
            return 0;
        }

        public int CfFree()
        {
            this.Dispose(false);
            return 0;
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



        #region Dispose

        bool disposed = false;


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any managed objects here.
                this.DisposeManaged();
            }

            // Free any unmanaged objects here.
            //
            this.DisposeUnManaged();
            this.DisposeSelf();
            disposed = true;
        }

        public void DisposeManaged()
        {

        }

        public void DisposeUnManaged()
        {
        }

        public void DisposeSelf()
        {


        }

        #endregion

    }
}
