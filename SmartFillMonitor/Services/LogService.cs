using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartFillMonitor.Services
{
    public static class LogService
    {
        public static void Info(string message) => Log.Logger.Information(message);
        public static void Warn(string message) => Log.Logger.Warning(message);
        public static void Debug(string message) => Log.Logger.Debug(message);
        public static void Verbose(string message) => Log.Logger.Verbose(message);
        public static void Fatal(string message) => Log.Logger.Fatal(message);
        public static void Fatal(string message,Exception ex = null) => Log.Logger.Information(ex,message);
        public static void Error(string message,Exception ex = null) => Log.Logger.Error(ex,message);
    

       
    }
}
