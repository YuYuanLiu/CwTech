using SensingNet.v0_1.Simulate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensingNet.CmdApp01
{
    class Program
    {
        public static void Main(string[] args)
        {
            using(var sim = new SNetSimulateSensorDeviceClient())
            {
                sim.RunAsyn();
                sim.CommandLine();
            }


        }
    }
}
