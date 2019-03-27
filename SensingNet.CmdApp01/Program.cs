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
using CToolkit.v1_0;

namespace SensingNet.CmdApp01
{
    class Program
    {
        public static void Main(string[] args)
        {
            using (var sim = CtkWcfDuplexTcpListener.CreateSingle())
            {
                sim.Uri = @"net.tcp://localhost:5000";

                sim.evtDataReceive += (ss, ee) =>
                {
                    var ea = ee as CtkWcfDuplexEventArgs;
                    Console.WriteLine(ea.WcfMsg.TypeName);
                };

                sim.ConnectIfNo();

                CtkCommandLine.Run();

            }


        }
    }
}
