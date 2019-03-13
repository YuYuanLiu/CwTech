using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.Dsp.Basic
{

    public class SNetDspWire
    {
        public List<SNetDspContact> Destinations = new List<SNetDspContact>();
        public SNetDspContact Source = new SNetDspContact();



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


    public class SNetDspWire<T> : SNetDspWire
        where T : EventArgs
    {


        //public new event EventHandler<T> EventTrigger;


    }


}
