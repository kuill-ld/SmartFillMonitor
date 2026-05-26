using System;
using System.Collections.Generic;
using System.Text;

namespace SmartFillMonitor.Services
{
    public static class DbProvider
    {
        private static readonly object _lock = new object();
        public static IFreeSql Fsql {  get; private set; }
        public static void Initialize(string connectionString,FreeSql.DataType data = FreeSql.DataType.Sqlite)
        {
            if (Fsql != null) return; 
            lock (_lock)
            {
                if (Fsql == null)
                {
                    Fsql = new FreeSql.FreeSqlBuilder()
                        .UseConnectionString(FreeSql.DataType.Sqlite, connectionString)
                        .UseAdoConnectionPool(true)
                        .UseMonitorCommand(
                        cmd => 
                        {
                            //sql执行前
                        },
                        (cmd, traceLog) => 
                        {
                            Console.WriteLine($"[SQL] {cmd.CommandText}\r\n->{traceLog}");
                        }
                        )
                        .UseAutoSyncStructure(true)
                        .Build();
                }
                return;
            }
        }
    }
}
