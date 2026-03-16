using Jellyfin.Plugin.ArtisanJelly.Services;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.ArtisanJelly
{
  public class PluginServiceRegistrator : IPluginServiceRegistrator
  {
    public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
    {
      serviceCollection.AddSingleton<ArtisanJellyService>();
      serviceCollection.AddSingleton<FilterService>();
      serviceCollection.AddSingleton<SearchManager>();
    }
  }
}

