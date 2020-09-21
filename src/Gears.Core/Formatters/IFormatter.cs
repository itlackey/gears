using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Gears.Formatters
{
    public interface IFormatter : IPlugin
    {
        //string Key { get; }

        string ContentType { get; }
        string DefaultFileExtension { get; }
        Task<string> GenerateContentAsync(PluginConfiguration formatterConfig, dynamic records);
    }
}
