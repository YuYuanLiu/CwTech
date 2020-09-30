using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra.Double;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SensingNet.v0_2.TdBase;
using SensingNet.v0_2.TdSignalProc;
using SensingNet.v0_2.TimeSignal;
using System;
using System.Linq;

namespace SensingNet.TestMy.UnitTest
{
    [TestClass]
    public class UtDspPropagate
    {

        [TestMethod]
        public void TestMethod()
        {

            var block = new SNetTdBlock();
            var node_seq = block.AddNode<SNetTdnSeqDataCollector>();
            var node_filter = block.AddNode<SNetTdnFilter>();
            var node_statistics = block.AddNode<SNetTdnStatistics>();

            node_seq.EhDataChange += node_filter.Input;
            node_filter.EhDataChange += node_statistics.Input;




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
                node_seq.TgInput(null, new SNetTdSignalSecSetF8EventArg()
                {
                    TSignalNew = new SNetTSignalSecF8() { Time = DateTime.Now, Signals = input.ToList() }
                });

                Console.Write("Avg={0}; Max={1}; Min={2}",
                    node_statistics.TSignalAvg.GetLastOrDefault(),
                    node_statistics.TSignalMax.GetLastOrDefault(),
                    node_statistics.TSignalMin.GetLastOrDefault());

            }





        }


    }
}
