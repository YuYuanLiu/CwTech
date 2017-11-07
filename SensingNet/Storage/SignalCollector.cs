using CToolkit.NumericProc;
using Cudafy.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace SensingNet.Storage
{
    /// <summary>
    /// 提供訊號收集器, 將每秒的資料收集起來
    /// 並提供一些簡易統計欄位, 在Refresh時更新
    /// 需被設置的資料為 times/signals
    /// </summary>
    [Serializable]
    public class SignalCollector : LinkedList<SignalPerSec>
    {
        public List<DateTime> times = new List<DateTime>();
        public List<double> signals = new List<double>();


        public ComplexD[] fft { get; private set; }
        public ComplexD[] spectrum { get; private set; }

        public double Max { get; private set; }
        public double Min { get; private set; }
        public double Avg { get; private set; }

        public ComplexD[] ComputeFft()
        {
            this.RefreshTime();
            if (this.signals.Count <= 0) { this.fft = new ComplexD[0]; return this.fft; }


            fft = new ComplexD[signals.Count];
            if (signals.Count <= 0) return fft;

            this.fft = NpContext.Singleton.FftForwardD(this.signals);
            return fft;
        }

        public ComplexD[] ComputeSpectrumH()
        {
            var fft = this.ComputeFft();


            var freqData = NpContext.Singleton.SpectrumFftD(fft);

            this.spectrum = new ComplexD[fft.Length / 2];
            for (int idx = 0; idx < spectrum.Length; idx++)
                spectrum[idx] = freqData[idx];


            return spectrum;
        }



        /// <summary>
        /// 僅更新時域資料
        /// </summary>
        public void RefreshTime()
        {
            this.signals.Clear();
            this.times.Clear();

            if (this.Count == 0)
            {
                this.Max = 0;
                this.Min = 0;
                this.Avg = 0;
                return;
            }

            foreach (var loop in this)
            {
                signals.AddRange(loop.signals);
                for (int idx = 0; idx < loop.signals.Count; idx++)
                {
                    times.Add(loop.dt);
                }
            }

            this.Max = this.signals.Max();
            this.Min = this.signals.Min();
            this.Avg = this.signals.Average();

        }

        /// <summary>
        /// 更新全部資料
        /// </summary>
        public void Refresh()
        {
            this.fft = null;
            this.spectrum = null;
            this.ComputeSpectrumH();
        }


        public void Interpolation(int dataSize)
        {
            foreach (var tfbps in this)
                tfbps.Interpolation(dataSize);
        }



        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}