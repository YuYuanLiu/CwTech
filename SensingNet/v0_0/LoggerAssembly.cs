using CToolkit.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_0
{
    public class LoggerAssembly
    {
        public static string LoggerAssemblyName { get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Name; } }
        public static Logger Logger { get { return LoggerMapper.Singleton.Get(LoggerAssemblyName); } }

        public static void Write(LoggerEventArgs ea)
        {
            Logger.Write(ea);
        }
        public static void Write(LoggerEventArgs ea, LoggerEnumLevel _level)
        {
            ea.level = _level;
            Logger.Write(ea);
        }
        //public static void Write(string msg, params object[] args) { Logger.Write(string.Format(msg, args)); }會造成呼叫模擬兩可

        public static void Verbose(string msg, params object[] args) { Logger.Write(string.Format(msg, args), LoggerEnumLevel.Verbose); }
        public static void Debug(string msg, params object[] args) { Logger.Write(string.Format(msg, args), LoggerEnumLevel.Debug); }
        public static void Info(string msg, params object[] args) { Logger.Write(string.Format(msg, args), LoggerEnumLevel.Info); }
        public static void Warn(string msg, params object[] args) { Logger.Write(string.Format(msg, args), LoggerEnumLevel.Warn); }
        public static void Error(string msg, params object[] args) { Logger.Write(string.Format(msg, args), LoggerEnumLevel.Error); }
        public static void Fatal(string msg, params object[] args) { Logger.Write(string.Format(msg, args), LoggerEnumLevel.Fatal); }
    }
}
