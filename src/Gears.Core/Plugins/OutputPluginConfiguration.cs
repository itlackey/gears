using System.Data;

namespace Gears
{
    public class OutputPluginConfiguration : PluginConfiguration
    {

        public string Name { get; set; }
        public bool Enabled { get; set; } = true;
        public PluginConfiguration Formatter { get; set; }

    }
}
