namespace Gears
{
    public interface IPluginFactory
    {
        TPlugin GetPlugin<TPlugin>(PluginConfiguration type) where TPlugin : IPlugin;
    }
}
