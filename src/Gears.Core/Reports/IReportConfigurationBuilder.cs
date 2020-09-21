using System.Collections.Generic;

namespace Gears.Reports
{
    public interface IReportConfigurationBuilder
    {
        Dictionary<string, ReportConfiguration> BuildReportConfigurations(IEnumerable<string> reportKeys);
    }
}
