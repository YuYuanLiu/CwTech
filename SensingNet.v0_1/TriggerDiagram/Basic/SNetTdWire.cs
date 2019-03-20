using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.TriggerDiagram.Basic
{

    public class SNetTdWire
    {
        public List<SNetTdContact> Destinations = new List<SNetTdContact>();
        public SNetTdContact Source = new SNetTdContact();



        public event EventHandler EventTrigger;

        public virtual void Trigger(object sender, EventArgs ea)
        {
        }
        protected virtual void OnTrigger(object sender, EventArgs ea)
        {
            if (this.EventTrigger == null) return;
            this.EventTrigger(this, ea);
        }
    }


    public class SNetDspWire<T> : SNetTdWire
        where T : EventArgs
    {


        //public new event EventHandler<T> EventTrigger;


    }


}
