using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_0.Secs
{
    /// <summary>
    /// Query Svid
    /// </summary>
    public class QSecsCfg
    {
        public String LocalIp;
        public int LocalPort = 5000;
        public String RemoteIp;
        public int RemotePort;

        public List<QSvidCfg> QSvidCfgList = new List<QSvidCfg>();


        public void SaveToXmlFile(string fn) { CToolkit.CtkUtil.SaveToXmlFileT(this, fn); }

    }

}
