using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace SensingNet.v0_1.QWcf
{

    [ServiceContract(Namespace = "http://Microsoft.ServiceModel.Samples", SessionMode = SessionMode.Required,
                    CallbackContract = typeof(ISNetQWcfClient))]
    public interface ISNetQWcfListener
    {

        [OperationContract(IsOneWay = true)]
        void Send(SNnetQWcfMessage msg);


    }



}
