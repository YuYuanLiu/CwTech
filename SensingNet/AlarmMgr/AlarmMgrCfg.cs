using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SensingNet.SecsMgr
{
    [Serializable]
    public class AlarmMgrCfg
    {

        public void SaveToFile() { SaveToFile(GetDefaultFileInfo().FullName); }
        public void SaveToFile(String fn)
        {
            var seri = new System.Xml.Serialization.XmlSerializer(typeof(SecsMgrCfg));
            var fi = new FileInfo(fn);


            using (var stm = fi.Open(FileMode.Create))
            {
                seri.Serialize(stm, this);
            }
        }


        public static AlarmMgrCfg LoadFromFile() { return LoadFromFile(GetDefaultFileInfo().FullName); }
        public static AlarmMgrCfg LoadFromFile(String fn)
        {
            var seri = new System.Xml.Serialization.XmlSerializer(typeof(AlarmMgrCfg));
            var fi = new FileInfo(fn);
            if (!fi.Exists)
            {
                var config = new AlarmMgrCfg();
                /*var dParam = new DeviceParam();
                config.DeviceParams.Add(dParam);
                dParam.SignalCfgList.Add(new SignalCfg());*/
                return config;
            }


            using (var stm = fi.OpenRead())
            {
                return seri.Deserialize(stm) as AlarmMgrCfg;
            }
        }


        static FileInfo GetDefaultFileInfo()
        {
            var fn = Assembly.GetExecutingAssembly().GetName().Name;
            fn += "." + typeof(AlarmMgrCfg).Name;
            fn += ".config";
            fn = Path.Combine("Config", fn);
            var fi = new FileInfo(fn);
            if (!fi.Directory.Exists)
                fi.Directory.Create();
            return fi;
        }



    }
}
