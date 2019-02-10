using CToolkit.v0_1;
using CToolkit.v0_1.Logging;
using CToolkit.v0_1.Net;
using CToolkit.v0_1.Secs;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SensingNet.v0_1.Simulate
{
    public class SNetSimulateSecsClient : IDisposable
    {
        CtkNonStopTcpClient client;
        public volatile bool IsSendRequest = false;

        ~SNetSimulateSecsClient() { this.Dispose(false); }

        public void RunAsyn()
        {

            client = new CtkNonStopTcpClient("127.0.0.1", 10002);
            client.evtFirstConnect += (ss, ee) => { Write("evtFirstConnect"); };
            client.evtFailConnect += (ss, ee) => { Write("evtFailConnect"); };
            client.evtErrorReceive += (ss, ee) => { Write("evtErrorReceive"); };
            client.evtDataReceive += (ss, ee) =>
            {
                Write("evtDataReceive");
            };

            client.NonStopConnectAsyn();
        }


        public void Write(string msg, params object[] obj)
        {
            Console.WriteLine();
            Console.WriteLine(msg, obj);
            Console.Write(">");
        }

        public void CommandLine()
        {
            var cmd = "";
            do
            {
                Console.Write(">");
                cmd = Console.ReadLine();

                switch (cmd)
                {
                    case "send":
                        this.Send();
                        break;
                    case "state":
                        Console.WriteLine("State={0}", this.client.IsRemoteConnected);
                        break;
                }


            } while (string.Compare(cmd, "exit", true) != 0);

            this.Stop();

        }

        public void Send()
        {
            var txMsg = new CtkHsmsMessage();
            txMsg.header.StreamId = 1;
            txMsg.header.FunctionId = 3;
            txMsg.header.WBit = true;
            var sList = new CtkSecsIINodeList();
            //var sSvid = new CToolkit.v0_1.Secs.SecsIINodeInt64();


            var list = new List<UInt64>();
            list.Add(0);
            list.Add(1);
            list.Add(2);
            list.Add(168);


            foreach (var scfg in list)
            {
                var sSvid = new CtkSecsIINodeUInt64();
                sSvid.Data.Add(scfg);
                sList.Data.Add(sSvid);
            }

            txMsg.rootNode = sList;

            this.client.WriteMsg(txMsg);

        }


        public void Stop()
        {
            if (this.client != null)
                this.client.AbortNonStopConnect();
        }


        #region IDisposable
        // Flag: Has Dispose already been called?
        protected bool disposed = false;

        // Public implementation of Dispose pattern callable by consumers.
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
                //
                this.DisposeManaged();
            }

            // Free any unmanaged objects here.
            //
            this.DisposeUnmanaged();

            this.DisposeSelf();

            disposed = true;
        }



        protected virtual void DisposeManaged()
        {
        }

        protected virtual void DisposeSelf()
        {
            this.Stop();
        }

        protected virtual void DisposeUnmanaged()
        {

        }
        #endregion


    }
}
