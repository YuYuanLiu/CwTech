using CToolkit.v0_1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_1.QSecs
{
    /// <summary>
    /// Query Svid
    /// </summary>
    public class SNetQSecsCfg
    {
        public String LocalIp;
        public int LocalPort = 5000;
        public String RemoteIp;
        public int RemotePort;

        public List<SNetQSvidCfg> QSvidCfgList = new List<SNetQSvidCfg>();


        public void SaveToXmlFile(string fn) { CtkUtil.SaveToXmlFileT(this, fn); }

    }

}
