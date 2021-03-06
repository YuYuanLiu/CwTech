﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;
using MathNet.Numerics;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using CToolkit.v1_1.Net;
using System.Threading;
using SensingNet.v0_2.DvcSensor;
using MathNet.Numerics.LinearAlgebra.Double;
using SensingNet.v0_2.TdSignalProc;
using SensingNet.v0_2.TimeSignal;
using System.Linq;
using SensingNet.v0_2.TdSecs;

namespace SensingNet.TestMy.UnitTest
{
    [TestClass]
    public class UtTdQSecs
    {

        [TestMethod]
        public void TestMethod()
        {

            var block = new SNetTdbQSecs();
            var node1 = block.AddNode<SNetTdnQSecs>();

            var isRunning = true;

            Task.Run(() =>
            {
                while (isRunning)
                {
                    node1.Input(this, new SNetTdSignalSecSetF8EventArg()
                    {

                    });

                    Thread.Sleep(1000);
                }
            });




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
                node1.Input(null, new SNetTdSignalSecSetF8EventArg()
                {
                    TSignalNew = new SNetTSignalSecF8(DateTime.Now, input.ToList())
                });


            }





        }


    }
}
