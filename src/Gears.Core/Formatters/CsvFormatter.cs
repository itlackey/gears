using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using CsvHelper;
using Microsoft.Extensions.Configuration;

namespace Gears.Formatters
{
    public class CsvFormatter : IFormatter
    {
        public string Key => "CSV";

        public string ContentType => "text/csv";

        public string DefaultFileExtension => ".csv";

        public Task<string> GenerateContentAsync(PluginConfiguration reportConfig, dynamic records)
        {
            var reportContent = string.Empty;
            using (var writer = new StringWriter())
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(records);
                return Task.FromResult(writer.ToString());
                //return Utils.WriteTempFile(reportKey, reportContent);
            }
        }
    }
}
