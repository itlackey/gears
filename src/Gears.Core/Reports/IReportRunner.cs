using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gears
{
    public interface IReportRunner
    {
        Task<IDictionary<string, ReportResult>> RunReportsAsync(IEnumerable<string> reports);
    }
}
