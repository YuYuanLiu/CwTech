using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;
using MathNet.Numerics;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CToolkit.v0_1.Numeric;

namespace SensingNet.MyTest
{
    [TestClass]
    public class UtTimeFreq
    {


        [TestMethod]
        public void Test()
        {
            var totalSecond = 64;
            var samplingRate = 256;
            var hslFactor = 0.7;
            //var length = samplingRate * totalSecond;
            var amp = 29.0 / hslFactor;



            using (var bmp = new Bitmap(totalSecond, samplingRate / 2))
            {
                var baseWave = Generate.Sinusoidal(samplingRate, samplingRate, 1, 24);
                for (int second = 0; second < totalSecond; second++)
                {

                    var abnormalWave = Generate.Sinusoidal(samplingRate, samplingRate, 1 * second, 5);


                    var finalWave = new double[baseWave.Length];
                    for (int idx = 0; idx < finalWave.Length; idx++)
                        finalWave[idx] = baseWave[idx] + abnormalWave[idx];

                    var npContext = new CToolkit.v0_1.Numeric.CtkNpContext();
                    var finalFft = npContext.FftForward(finalWave);

                    var spectrum = npContext.SpectrumHalfFft(finalFft);

                    for (int hz = 0; hz < spectrum.Count; hz++)
                    {
                        bmp.SetPixel(second, spectrum.Count - hz - 1
                            , CToolkit.v0_1.Paint.CtkRgbColor.FromHSL(hslFactor - spectrum[hz].Magnitude / amp, 0.8, 0.5));
                    }

                }
                bmp.Save("spectrum.bmp");
            }






        }


        [TestMethod]
        public void Test2()
        {
            //var samplingRate = 1000;
            var hslFactor = 0.7;
            //var length = samplingRate * totalSecond;
            var amp = 29.0 / hslFactor;
            var window = 128;


            var datacsv = File.ReadAllText("data.csv");
            var data = datacsv.Split(new char[] { ',' });
            var wave = new List<double>();
            foreach (var val in data)
                wave.Add(double.Parse(val));

            var hamming = MathNet.Numerics.Window.Hamming(512);


            var npContext = new CToolkit.v0_1.Numeric.CtkNpContext();
            using (var bmp = new Bitmap(wave.Count / window, window / 2))
            {
                var spec = new List<List<double>>();

                for (int second = 0; second < bmp.Width; second++)
                {
                    var signal = new List<double>();
                    for (int idx = 0; idx < window; idx++)
                    {
                        var val = wave[second * window + idx];
                        signal.Add(val);
                    }



                    var fft = npContext.FftForward(signal);
                    var spectrumD = npContext.SpectrumHalfFft(fft);
                    var spectrum = CToolkit.v0_1.Numeric.CtkTypeConverter.ToMagnitude(spectrumD);
                    spectrum = CtkNumUtil.Interpolation(spectrum, bmp.Height);

                    var max = spectrum.Max();
                    for (int idx = 0; idx < spectrum.Length; idx++)
                    {
                        spectrum[idx] = spectrum[idx] * hamming[(int)(spectrum[idx] / max * (hamming.Length - 1))];
                    }



                    spec.Add(new List<double>(spectrum));
                }

                amp = 0;
                foreach (var row in spec)
                    foreach (var col in row)
                        amp = amp < col ? col : amp;


                for (int second = 0; second < spec.Count; second++)
                    for (int hz = 0; hz < spec[second].Count; hz++)
                    {
                        var row = spec[second];
                        var col = row[hz];

                        bmp.SetPixel(second, row.Count - hz - 1
                            , CToolkit.v0_1.Paint.CtkJetColor.Color(col + 0.1, 0, amp));
                    }

                bmp.Save("spectrum.bmp");
            }






        }


    }
}
