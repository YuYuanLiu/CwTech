using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;
using MathNet.Numerics;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using SensingNet.v0_1.Storage;
using CToolkit.v0_1.Net;
using System.Threading;
using SensingNet.v0_1.Protocol;
using SensingNet.v0_1.Device;
using SensingNet.v0_1.Dsp.Block;
using MathNet.Numerics.LinearAlgebra.Double;

namespace SensingNet.MyTest.UnitTest
{
    [TestClass]
    public class UtDspBlockPropagate
    {

        [TestMethod]
        public void TestMethod()
        {

            var block_seq = new SNetDspBlockSeqDataCollector();
            var block_filter = new SNetDspBlockFilter();
            var block_statistics = new SNetDspBlockBasicStatistics();

            block_filter.Input = block_seq;
            block_statistics.Input = block_filter;


            var len = 512;
            var sampleRate = 512.0;
            var sin1 = new DenseVector(Generate.Sinusoidal(len, sampleRate, 10, 1.0));
            var sin2 = new DenseVector(Generate.Sinusoidal(len, sampleRate, 100.0, 0.5));
            var wave = sin1 + sin2;


            var dt = DateTime.Now;
            var rnd = new Random((int)dt.Ticks);
            var input = new DenseVector(len);

            for (var times = 0; times < 10; times++)
            {
                dt = dt.AddSeconds(1);

                for (var idx = 0; idx < input.Count; idx++)
                    input[idx] = rnd.NextDouble() * 0.2;

                input += wave;
                block_seq.Input(input, dt);

                Console.Write("Avg={0}; Max={1}; Min={2}", 
                    block_statistics.TSignalAvg.GetLastOrDefault(),
                    block_statistics.TSignalMax.GetLastOrDefault(),
                    block_statistics.TSignalMin.GetLastOrDefault());

            }





        }


    }
}
