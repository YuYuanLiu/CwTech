using CToolkit.v0_1.Wcf.Example;
using SensingNet.v0_1.QWcf.Simulate;
using SensingNet.v0_1.Simulate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace SensingNet.CmdApp02
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var example = new SNetSimulateQWcfClient())
            {
                example.RunAsyn();
                example.CommandLine();
            }



        }
    }
}
