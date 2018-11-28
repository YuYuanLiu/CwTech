using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace SensingNet.v0_1.Protocol
{

    public interface IProtoConnectBase
    {


        bool IsConnected { get; }
        bool IsConnecting { get; }

        void ConnectIfNo();
        void NonStopConnect();


    }
}