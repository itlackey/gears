using System.Collections;
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

        public Task<string> GenerateContentAsync(PluginConfiguration reportConfig, dynamic input)
        {
            using (var writer = new StringWriter())
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                var records = input as IEnumerable;
                if (records == null)
                    csv.WriteRecord(input);
                else
                    csv.WriteRecords(records);

                return Task.FromResult(writer.ToString());
            }
        }
    }
}
