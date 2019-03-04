using CToolkit.v0_1.Wcf.Example;
using SensingNet.v0_1.Wcf.Simulate;
using SensingNet.v0_1.Simulate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using CToolkit.v0_1.Wcf;

namespace SensingNet.CmdApp02
{
    class Program
    {
        public static void Main(string[] args)
        {
            using (var example = new SNetSimulateDeviceRandom())
            {
                example.RunAsyn();
                example.CommandLine();
            }


        }
    }
}
