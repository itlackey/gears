using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Gears
{
    public class ReportConfiguration //: ConfigurationSection
    {
        // public ReportConfiguration()
        //     : base(new ConfigurationRoot(new List<IConfigurationProvider>()), "Reports")
        // {

        // }
        // public ReportConfiguration(IConfigurationRoot root, string path) : base(root, path)
        // {
        // }

        public string ReportName { get; set; }

        public IEnumerable<string> PluginPaths { get; set; } = new string[] { };

        public bool Enabled { get; set; } = true;

        public PluginConfiguration Input { get; set; }

        public IConfigurationSection Args { get; set; }
        public PluginConfiguration Transformer { get; set; }

        public Dictionary<string, OutputPluginConfiguration> Output { get; set; }

    }
}
