﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_2.TdSignalProc
{
    public class SNetTdNPass: SNetTdNodeF8
    {

        public void TgInput(object sender, SNetTdSignalSecF8EventArg ea)
        {
            if (!this.IsEnalbed) return;
            this.OnDataChange(ea);
        }
        public void TgInput(object sender, SNetTdSignalSetSecF8EventArg ea)
        {
            if (!this.IsEnalbed) return;
            this.OnDataChange(ea);
        }
    }
}