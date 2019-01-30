using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace SensingNet.v0_1.Wcf
{


    public interface ISNetWcfClient
    {
        [OperationContract(IsOneWay = true)]
        void Send(SNnetWcfMessage msg);

        [OperationContract()]
        SNnetWcfMessage SendRelay(SNnetWcfMessage msg);

    }


}
