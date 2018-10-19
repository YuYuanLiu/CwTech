using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SensingNet.v0_1.Framework
{
    [Serializable]
    public class DeviceMgrCfg
    {


        public void SaveToFile() { SaveToFile(GetDefaultFileInfo().FullName); }
        public void SaveToFile(String fn)
        {
            var seri = new System.Xml.Serialization.XmlSerializer(typeof(DeviceMgrCfg));
            var fi = new FileInfo(fn);


            using (var stm = fi.Open(FileMode.Create))
            {
                seri.Serialize(stm, this);
            }
        }


        public static DeviceMgrCfg LoadFromFile() { return LoadFromFile(GetDefaultFileInfo().FullName); }
        public static DeviceMgrCfg LoadFromFile(String fn)
        {
            var seri = new System.Xml.Serialization.XmlSerializer(typeof(DeviceMgrCfg));
            var fi = new FileInfo(fn);
            if (!fi.Exists)
            {
                var config = new DeviceMgrCfg();
                /*var dParam = new DeviceParam();
                config.DeviceParams.Add(dParam);
                dParam.SignalCfgList.Add(new SignalCfg());*/
                return config;
            }


            using (var stm = fi.OpenRead())
            {
                return seri.Deserialize(stm) as DeviceMgrCfg;
            }
        }


        static FileInfo GetDefaultFileInfo()
        {
            var fn = Assembly.GetExecutingAssembly().GetName().Name;
            fn += "." + typeof(DeviceMgrCfg).Name;
            fn += ".config";
            fn = Path.Combine("Config", fn);
            var fi = new FileInfo(fn);
            if (!fi.Directory.Exists)
                fi.Directory.Create();
            return fi;
        }



    }
}
