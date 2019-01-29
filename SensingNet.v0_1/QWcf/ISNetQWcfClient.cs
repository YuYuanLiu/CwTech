using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace SensingNet.v0_1.QWcf
{


    public interface ISNetQWcfClient
    {
        [OperationContract(IsOneWay = true)]
        void Send(SNnetQWcfMessage msg);

        [OperationContract()]
        SNnetQWcfMessage SendRelay(SNnetQWcfMessage msg);

    }


}
