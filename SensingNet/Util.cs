using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace SensingNet
{
    public class Util
    {


        
        public static int DoBgWorkerAsync(DoWorkEventHandler method)
        {
            var bg = new BackgroundWorker();
            bg.DoWork += method;
            bg.WorkerSupportsCancellation = true;
            bg.RunWorkerAsync();

            return 0;
        }



    }
}
