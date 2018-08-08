using CToolkit.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet
{
    public class LoggerAssembly
    {
        public static string LoggerAssemblyName { get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Name; } }
        public static Logger Logger { get { return LoggerMapper.Singleton[LoggerAssemblyName]; } }


        public static void Write(LoggerEventArgs ea)
        {
            Logger.Write(ea);
        }

        public static void Write(String format, params object[] args)
        {
            Logger.Write(string.Format(format, args));
        }
    }
}
