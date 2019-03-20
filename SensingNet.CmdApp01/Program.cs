using CToolkit.v1_0.Wcf.Example;
using SensingNet.v0_1.Wcf.Simulate;
using SensingNet.v0_1.Simulate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using CToolkit.v1_0.Wcf;
using SensingNet.v0_1.Device.Simulate;

namespace SensingNet.CmdApp01
{
    class Program
    {
        public static void Main(string[] args)
        {
            using (var example = new SNetSimulateSensorDeviceClient())
            {
                example.RunAsyn();
                example.CommandLine();
            }


        }
    }
}
