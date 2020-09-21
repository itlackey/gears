using System;
using Microsoft.Extensions.Configuration;

namespace Gears
{
    public class PluginConfiguration
    {
        public string Type { get; set; }
        public IConfigurationSection Args { get; set; }
        public string ReportName { get; set; }
    }
}
