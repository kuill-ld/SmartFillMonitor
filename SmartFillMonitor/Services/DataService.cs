using SmartFillMonitor.Models;
using System;
using System.Collections.Generic;
using System.Text;
using CsvHelper;
using System.IO;
using System.Globalization;
namespace SmartFillMonitor.Services
{
    public static class DataService
    {
        public static async Task SaveProductionRecordAsync(ProductionRecord record)
        {
            await DbProvider.Fsql.Insert(record).ExecuteAffrowsAsync();
        }
        public static async Task<List<ProductionRecord>> QueryRecordAsync(DateTime start, DateTime end)
        {
           return await DbProvider.Fsql.Select<ProductionRecord>().Where(r => r.Time >= start && r.Time <= end).ToListAsync();
        }
        public static async Task ExportToAsync(List<ProductionRecord> records, string filePath)
        {
            await using var writer = new StreamWriter(filePath);
            await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            await csv.WriteRecordsAsync(records);
        }
    }
}
