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
using MathNet.Numerics.LinearAlgebra.Double;
using SensingNet.v0_1.Dsp;
using SensingNet.v0_1.Dsp.Basic;
using SensingNet.v0_1.Dsp.TimeSignal;
using System.Linq;

namespace SensingNet.MyTest.UnitTest
{
    [TestClass]
    public class UtDspPropagate
    {

        [TestMethod]
        public void TestMethod()
        {

            var block = new SNetDspBlock();
            var node_seq = block.AddNode<SNetDspNodeSeqDataCollector>();
            var node_filter = block.AddNode<SNetDspNodeFilter>();
            var node_statistics = block.AddNode<SNetDspNodeStatistics>();

            node_seq.evtDataChange += node_filter.DoInput;
            node_filter.evtDataChange += node_statistics.DoInput;




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
                node_seq.DoInput(null, new SNetDspSignalSetSecF8EventArg()
                {
                    NewTSignal = new SNetDspTSignalSecF8() { Time = DateTime.Now, Signals = input.ToList() }
                });

                Console.Write("Avg={0}; Max={1}; Min={2}",
                    node_statistics.TSignalAvg.GetLastOrDefault(),
                    node_statistics.TSignalMax.GetLastOrDefault(),
                    node_statistics.TSignalMin.GetLastOrDefault());

            }





        }


    }
}
