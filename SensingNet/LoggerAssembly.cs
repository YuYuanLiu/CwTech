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
        public static Logger Logger { get { return LoggerMapper.Singleton.Get(LoggerAssemblyName); } }

        public static void Write(LoggerEventArgs ea)
        {
            Logger.Write(ea);
        }
        public virtual void Write(LoggerEventArgs ea, LoggerEnumLevel _level)
        {
            ea.level = _level;
            Logger.Write(ea);
        }


        public virtual void Verbose(string msg, params object[] args) { Logger.Write(string.Format(msg, args), LoggerEnumLevel.Verbose); }
        public virtual void Debug(string msg, params object[] args) { Logger.Write(string.Format(msg, args), LoggerEnumLevel.Debug); }
        public virtual void Info(string msg, params object[] args) { Logger.Write(string.Format(msg, args), LoggerEnumLevel.Info); }
        public virtual void Warn(string msg, params object[] args) { Logger.Write(string.Format(msg, args), LoggerEnumLevel.Warn); }
        public virtual void Error(string msg, params object[] args) { Logger.Write(string.Format(msg, args), LoggerEnumLevel.Error); }
        public virtual void Fatal(string msg, params object[] args) { Logger.Write(string.Format(msg, args), LoggerEnumLevel.Fatal); }
    }
}
