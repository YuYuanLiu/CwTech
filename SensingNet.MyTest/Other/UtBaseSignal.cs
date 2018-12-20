using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;
using MathNet.Numerics;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace SensingNet.MyTest
{
    [TestClass]
    public class UtBaseSignal
    {


        [TestMethod]
        public void TestMethod2()
        {
            var length = 2048;
            var samplingRate = 512;
            var baseWave = Generate.Sinusoidal(length, samplingRate, 1, 5);
            var abnormalWave = Generate.Sinusoidal(length, samplingRate, 32, 10);

            var finalWave = new double[baseWave.Length];
            for (int idx = 0; idx < finalWave.Length; idx++)
            {
                var a = baseWave[idx];
                var b = abnormalWave[idx];
                finalWave[idx] = a + b;
            }


            var npContext = new CToolkit.v0_1.NumericProc.CtkNpContext();
            var baseFft = npContext.FftForward(baseWave);
            var abnormalFft = npContext.FftForward(abnormalWave);
            var finalFft = npContext.FftForward(finalWave);


            var hipass = MathNet.Filtering.FIR.FirCoefficients.BandPass(samplingRate, 16, 64);
            var filter = new MathNet.Filtering.FIR.OnlineFirFilter(hipass);
            var hipassWave = new double[finalWave.Length];
            for (int idx = 0; idx < finalWave.Length; idx++)
            {
                hipassWave[idx] = filter.ProcessSample(finalWave[idx]);
            }


            using (var f = File.Open("data.csv", FileMode.Create))
            using (var sw = new StreamWriter(f))
            {
                sw.WriteLine("Wave,a,b,a+b,f(a),f(b),f(a+b),correct");
                var trendSignal = new List<double>();

                for (int idx = 0; idx < baseWave.Length; idx++)
                {
                    trendSignal.Add(finalWave[idx]);
                    for (int tsidx = 0; tsidx < 99; tsidx++)
                        if (trendSignal.Count > 32)
                            trendSignal.RemoveAt(0);

                    sw.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7}",
                        idx,
                        baseWave[idx],
                        abnormalWave[idx],
                        finalWave[idx],
                        baseFft[idx].Magnitude,
                        abnormalFft[idx].Magnitude,
                        finalFft[idx].Magnitude,
                        hipassWave[idx]
                        );
                }
            }


        }

        [TestMethod]
        public void ContinueSignalFFt()
        {
            var length = 2048;
            var samplingRate = 512;
            var baseWave = Generate.Sinusoidal(length, samplingRate, 1, 24);
            var abnormalWave = Generate.Sinusoidal(length, samplingRate, 10, 5);

            var base2Wave = new List<double>();
            base2Wave.AddRange(baseWave);
            for (int idx = 0; idx < baseWave.Length; idx++)
                base2Wave.Add(0);
            var abnormal2Wave = new List<double>();
            for (int idx = 0; idx < abnormalWave.Length; idx++)
                abnormal2Wave.Add(0);
            abnormal2Wave.AddRange(abnormalWave);

            var finalWave = new List<double>();
            finalWave.AddRange(baseWave);
            finalWave.AddRange(abnormalWave);


            var npContext = new CToolkit.v0_1.NumericProc.CtkNpContext();
            var baseFft = npContext.SpectrumTime(baseWave);
            var abnormalFft = npContext.SpectrumTime(abnormalWave);
            var finalFft = npContext.SpectrumTime(finalWave);

            var base2Fft = npContext.SpectrumTime(base2Wave);
            var abnormal2Fft = npContext.SpectrumTime(abnormal2Wave);




            using (var f = File.Open("data2.csv", FileMode.Create))
            using (var sw = new StreamWriter(f))
            {
                sw.WriteLine("Wave,bw,aw,bf,af,,Wave,fw,ff,b2f,a2f");
                for (int idx = 0; idx < finalWave.Count; idx++)
                {
                    var v0 = idx < baseWave.Length ? idx + "" : "";
                    var bw = idx < baseWave.Length ? baseWave[idx] + "" : "";
                    var aw = idx < abnormalWave.Length ? abnormalWave[idx] + "" : "";
                    var bf = idx < baseFft.Count ? baseFft[idx].Magnitude + "" : "";
                    var af = idx < abnormalFft.Count ? abnormalFft[idx].Magnitude + "" : "";
                    var fw = finalWave[idx];
                    var ff = finalFft[idx].Magnitude;

                    var b2f = idx < base2Fft.Count ? base2Fft[idx].Magnitude + "" : "";
                    var a2f = idx < abnormal2Fft.Count ? abnormal2Fft[idx].Magnitude + "" : "";


                    sw.WriteLine("{0},{1},{2},{3},{4},,{5},{6},{7},{8},{9}",
                        v0, bw, aw, bf, af,
                        idx / 2.0, fw, ff, b2f, a2f
                        );
                }
            }


        }

    }
}
