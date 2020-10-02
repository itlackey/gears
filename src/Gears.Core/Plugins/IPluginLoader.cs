namespace Gears.Core.Plugins
{
    public interface IPluginLoader
    {
        void ScanAndLoad(string[] pluginPaths);
    }
}
