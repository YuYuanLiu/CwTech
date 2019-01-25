using CToolkit.v0_1.Wcf.Example;
using SensingNet.v0_1.QWcf.Simulate;
using SensingNet.v0_1.Simulate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace SensingNet.CmdApp01
{
    class Program
    {
        public static void Main(string[] args)
        {
            using (var example = new SNetSimulateQWcfListener())
            {
                example.RunAsyn();
                example.CommandLine();
            }


        }
    }
}
