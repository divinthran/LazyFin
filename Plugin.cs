using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.MultiSourceScraper
{
    public class Plugin : BasePlugin<PluginConfiguration>
    {
        public override string Name => "Multi-Source Scraper & Streamer";
        public override Guid Id => Guid.Parse("a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d");

        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
            : base(applicationPaths, xmlSerializer)
        {
            Instance = this;
        }

        public static Plugin? Instance { get; private set; }
    }

    public class PluginConfiguration : BasePluginConfiguration
    {
        public string VidkingApiKey { get; set; } = string.Empty;
        public bool Prefer4K { get; set; } = true;
    }
}