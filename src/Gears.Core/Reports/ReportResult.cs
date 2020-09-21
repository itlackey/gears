using System.Collections.Generic;

namespace Gears
{
    public class ReportResult
    {
        public int Status { get; set; } = 0;
        public string Message { get; set; }

        public IEnumerable<dynamic> Data { get; set; }
    }
}