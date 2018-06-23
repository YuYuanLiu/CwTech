using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;
using MathNet.Numerics;
using System.IO;
using System.Collections.Generic;

namespace SensingNet.MyTest
{
    [TestClass]
    public class UtSampleCaptureSignal
    {


        [TestMethod]
        public void TestMethod()
        {
            var length = 2048;
            var samplingRate = 512;
            var baseWave = Generate.Sinusoidal(length, samplingRate, 1, 24);
            var abnormalWave = Generate.Sinusoidal(length, samplingRate, 16, 5);

            var finalWave = new double[baseWave.Length];
            for (int idx = 0; idx < finalWave.Length; idx++)
            {
                var a = baseWave[idx];
                var waveLoopIdx = idx % samplingRate;
                var b = (waveLoopIdx > samplingRate / 2 - 32) && (waveLoopIdx < samplingRate / 2 + 32) ? abnormalWave[idx] : 0;
                abnormalWave[idx] = b;
                finalWave[idx] = a + b;
            }




            var npContext = new CToolkit.NumericProc.NpContext();
            var baseFft = npContext.FftForward(baseWave);
            var abnormalFft = npContext.FftForward(abnormalWave);
            var finalFft = npContext.FftForward(finalWave);


            using (var f = File.Open("data.csv", FileMode.Create))
            using (var sw = new StreamWriter(f))
            {
                sw.WriteLine("Wave,a,b,a+b");
                for (int idx = 0; idx < baseWave.Length; idx++)
                {


                    sw.WriteLine("{0},{1},{2},{3},{4},{5},{6}",
                        idx,
                        baseWave[idx],
                        abnormalWave[idx],
                        finalWave[idx],
                        baseFft[idx].Magnitude,
                        abnormalFft[idx].Magnitude,
                        finalFft[idx].Magnitude
                        );
                }
            }


        }


    }
}
