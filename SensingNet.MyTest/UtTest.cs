using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;
using MathNet.Numerics;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Globalization;
using System.Text;

namespace SensingNet.MyTest
{
    [TestClass]
    public class UtTest
    {


        [TestMethod]
        public void TestMethod()
        {
            var sb = new StringBuilder();

            sb.AppendLine("aaaaaaa");
            sb.Append("bbbb");

            var line = "";
            var queue = new Queue<string>();
            using (var sr = new StringReader(sb.ToString()))
            {
                for (line = sr.ReadLine(); line != null; line = sr.ReadLine())
                {
                    if (line.IndexOf("\n") < 0) break;

                    line = line.Replace("\r", "");
                    line = line.Replace("\n", "");
                    line = line.Trim();
                    lock (this)
                        queue.Enqueue(line);
                    line = "";
                }
            }

            lock (this)
            {
                sb.Clear();
                sb.Append(line);
            }


        }


    }
}
