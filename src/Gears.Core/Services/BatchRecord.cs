using System;

namespace Gears.Services
{
    public class BatchRecord
    {
        public int Id { get; set; }
        public string Report { get; set; }
        public string Output { get; set; }
        public byte[] Hash { get; set; }
        public DateTime? LastReported { get; set; }
    }
}