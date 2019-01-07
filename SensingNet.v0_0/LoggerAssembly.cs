using CToolkit.v0_1.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensingNet.v0_0
{
    public class LoggerAssembly
    {
        public static string LoggerAssemblyName { get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Name; } }
        public static CtkLogger Logger { get { return CtkLoggerMapper.Singleton.Get(LoggerAssemblyName); } }

        public static void Write(CtkLoggerEventArgs ea)
        {
            Logger.Write(ea);
        }
        public static void Write(CtkLoggerEventArgs ea, CtkLoggerEnumLevel _level)
        {
            ea.Level = _level;
            Logger.Write(ea);
        }
        //public static void Write(string msg, params object[] args) { Logger.Write(string.Format(msg, args)); }會造成呼叫模擬兩可

        public static void Verbose(string msg, params object[] args) { Logger.Write(string.Format(msg, args), CtkLoggerEnumLevel.Verbose); }
        public static void Debug(string msg, params object[] args) { Logger.Write(string.Format(msg, args), CtkLoggerEnumLevel.Debug); }
        public static void Info(string msg, params object[] args) { Logger.Write(string.Format(msg, args), CtkLoggerEnumLevel.Info); }
        public static void Warn(string msg, params object[] args) { Logger.Write(string.Format(msg, args), CtkLoggerEnumLevel.Warn); }
        public static void Error(string msg, params object[] args) { Logger.Write(string.Format(msg, args), CtkLoggerEnumLevel.Error); }
        public static void Fatal(string msg, params object[] args) { Logger.Write(string.Format(msg, args), CtkLoggerEnumLevel.Fatal); }
    }
}
