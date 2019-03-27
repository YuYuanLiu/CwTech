using CToolkit.v1_0.Wcf.Example;
using SensingNet.v0_1.Wcf.Simulate;
using SensingNet.v0_1.Simulate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using CToolkit.v1_0.Wcf;
using CToolkit.v1_0;

namespace SensingNet.CmdApp02
{



    class Program
    {
        public static void Main(string[] args)
        {
            using (var sim = CtkWcfDuplexTcpClient.CreateSingle())
            {
                sim.Uri = "net.tcp://localhost:5000";
                sim.ConnectIfNo();

                var msg = CtkWcfMessage.Create(new CtkWcfTestUri());

                sim.Channel.CtkSend(msg);

                CtkCommandLine.Run();
            }


        }
    }
}
