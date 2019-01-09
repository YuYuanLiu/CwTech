using SensingNet.v0_1.Simulate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensingNet.CmdApp02
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var sim = new SNetSimulateVibration())
            {
                sim.RunAsyn();
                sim.CommandLine();
            }
        }
    }
}
