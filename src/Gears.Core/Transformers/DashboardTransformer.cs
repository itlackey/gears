using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Gears.Transformers
{
    internal class DashboardTransformer : ITransformer
    {
        public string Key => "Dashboard";

        public dynamic Transform(string reportKey, dynamic input, PluginConfiguration transformerConfig)
        {
            var records = input.MGITest as IEnumerable<dynamic>;

            var totalPositives = records.Select(r => (decimal)(r.TotalPositives)).Sum(r => r);
            var totalTests = records.Select(r => (decimal)(r.TotalTests)).Sum(r => r);
            var totalWeeklyPositives = records.Where(r => r["Date"] >= DateTime.Now.AddDays(-7))
                .Select(r => (decimal)(r.TotalPositives)).Sum(r => r);
            var totalWeeklyTests = records.Where(r => r["Date"] >= DateTime.Now.AddDays(-7))
                .Select(r => (decimal)(r.TotalTests)).Sum(r => r);
            //var data = records.FirstOrDefault();

            foreach (var record in records)
            {
                record.DisplayDate = record.Date.ToUniversalTime().ToShortDateString();
                record.PositivityRate = ((decimal)record.TotalPositives / (decimal)record.TotalTests).ToString();
            }

            var data = new Dictionary<string, object>()
            {
                { "StartDate", records.Min(r => r.Date).ToShortDateString()},
                { "WeekStartDate", records.Where(r => r.Date >= DateTime.Now.AddDays(-7)).Min(r => r.Date).ToShortDateString()},
                { "WeekEndDate", records.Max(r => r.Date).ToShortDateString()},
                { "TotalTests", records.Sum(r => r.TotalTests)},
                { "TotalStudentPositives", 2},
                { "TotalEmployeePositives", totalPositives -2},
                { "TotalPositives",  totalPositives},
                { "WeeklyStudentPositives", 2},
                { "WeeklyEmployeePositives", totalWeeklyPositives -2},
                { "WeeklyTotalPositives",  totalWeeklyPositives},
                { "PositivityRate" , string.Format("{0:P2}", (totalPositives / totalTests)) },
                { "WeeklyPositivityRate" , string.Format("{0:P2}", (totalWeeklyPositives / totalWeeklyTests)) },
                {
                    "Data", JsonSerializer.Serialize(records , new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        WriteIndented = true
                    })
                }
            };

            return data;
        }
    }
}
