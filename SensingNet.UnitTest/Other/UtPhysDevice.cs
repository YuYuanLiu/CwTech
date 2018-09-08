using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;
using CToolkit.Secs;
using System.Net;
using SensingNet.v0_0;
using SensingNet.v0_0.Secs;

namespace SensingNet.MyTest
{
    [TestClass]
    public class UtPhysDevice
    {
        [TestMethod]
        public void SecsTest()
        {

            using (var signalmgr = new v0_0. Signal.SignalMgrExecer())
            using (var secsmgr = new v0_0.Secs.SecsMgrExecer())
            {


                signalmgr.evtSignalCapture += delegate (object sender, SignalEventArgs e)
                {
                    
                };
                CToolkit.CtkUtil.RunWorkerAsyn(delegate (object sen, DoWorkEventArgs dwea)
                {
                    signalmgr.CfInit();
                    signalmgr.CfLoad();
                    signalmgr.CfRun();
                    signalmgr.CfUnLoad();
                    signalmgr.CfFree();
                });



                secsmgr.evtReceiveData += delegate (object sender, EventArgsSecsRcvData e)
                {
                    var myMsg = e.message;
                    if (myMsg.header.StreamId == 1 && myMsg.header.FunctionId == 3)
                    {
                        var rootNode = myMsg.rootNode as SecsIINodeList;
                        var svidNode = rootNode.Data[0] as SecsIINodeUInt32;
                        var qsvid = svidNode.Data[0];

                        

                        var msg = new HsmsMessage();
                        msg.header.StreamId = 1;
                        msg.header.FunctionId = 4;
                        var list = new SecsIINodeList();
                        var signal = new SecsIINodeASCII();
                        signal.AddString(1.2 + "");
                        list.Data.Add(signal);
                        msg.rootNode = list;

                        e.handler.hsmsConnector.Send(msg);
                    }


                };

                secsmgr.CfInit();
                secsmgr.CfLoad();
                secsmgr.CfRun();
                secsmgr.CfUnLoad();
                secsmgr.CfFree();


                signalmgr.isExec = false;

                System.Diagnostics.Debug.WriteLine("");

            }


        }



        [TestMethod]
        public void HsmsConnectorTestPassive()
        {
            using (var hsmsConnector = new HsmsConnector())
            {
                hsmsConnector.evtReceiveData += delegate (Object sen, HsmsConnector_EventArgsRcvData evt)
                {

                    var myMsg = evt.msg;


                    System.Diagnostics.Debug.WriteLine("S{0}F{1}", myMsg.header.StreamId, myMsg.header.FunctionId);
                    System.Diagnostics.Debug.WriteLine("SType= {0}", myMsg.header.SType);

                    switch (myMsg.header.SType)
                    {
                        case 1:
                            hsmsConnector.Send(HsmsMessage.CtrlMsg_SelectRsp(0));
                            return;
                        case 5:
                            hsmsConnector.Send(HsmsMessage.CtrlMsg_LinktestRsp());
                            return;
                    }

                    if (myMsg.header.StreamId == 1 && myMsg.header.FunctionId == 3)
                    {
                        var msg = new HsmsMessage();
                        msg.header.StreamId = 1;
                        msg.header.FunctionId = 4;
                        var list = new SecsIINodeList();
                        var signal = new SecsIINodeFloat64();
                        signal.Data.Add(1.2);
                        list.Data.Add(signal);
                        msg.rootNode = list;

                        hsmsConnector.Send(msg);
                    }

                };




                //hsmsConnector.ctkConnSocket.isActively = true;
                hsmsConnector.local = new IPEndPoint(IPAddress.Parse("192.168.217.1"), 5000);
                hsmsConnector.remote = new IPEndPoint(IPAddress.Parse("192.168.217.129"), 5000);
                for (int idx = 0; true; idx++)
                {
                    try
                    {
                        hsmsConnector.Connect();
                        hsmsConnector.ReceiveRepeat();
                    }
                    catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.StackTrace); }
                }

            }
        }

        [TestMethod]
        public void HsmsConnectorTestActive()
        {
            using (var hsmsConnector = new HsmsConnector())
            {
                hsmsConnector.evtReceiveData += delegate (Object sen, HsmsConnector_EventArgsRcvData evt)
                {

                    var myMsg = evt.msg;


                    System.Diagnostics.Debug.WriteLine("S{0}F{1}", myMsg.header.StreamId, myMsg.header.FunctionId);
                    System.Diagnostics.Debug.WriteLine("SType= {0}", myMsg.header.SType);

                    switch (myMsg.header.SType)
                    {
                        case 1:
                            hsmsConnector.Send(HsmsMessage.CtrlMsg_SelectRsp(0));
                            return;
                        case 5:
                            hsmsConnector.Send(HsmsMessage.CtrlMsg_LinktestRsp());
                            return;
                    }

                    if (myMsg.header.StreamId == 1 && myMsg.header.FunctionId == 3)
                    {
                        var msg = new HsmsMessage();
                        msg.header.StreamId = 1;
                        msg.header.FunctionId = 4;
                        var list = new SecsIINodeList();
                        var signal = new SecsIINodeFloat64();
                        signal.Data.Add(1.2);
                        list.Data.Add(signal);
                        msg.rootNode = list;

                        hsmsConnector.Send(msg);
                    }

                };




                hsmsConnector.connSocket.isActively = true;
                hsmsConnector.local = new IPEndPoint(IPAddress.Parse("192.168.217.1"), 5000);
                hsmsConnector.remote = new IPEndPoint(IPAddress.Parse("192.168.217.129"), 5000);
                for (int idx = 0; true; idx++)
                {
                    try
                    {
                        hsmsConnector.Connect();
                        hsmsConnector.Send(HsmsMessage.CtrlMsg_SelectReq());
                        hsmsConnector.ReceiveRepeat();
                    }
                    catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.StackTrace); }
                }

            }




        }

    }
}
