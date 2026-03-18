using System;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.ArtisanJelly.Configuration
{
    /// <summary>
    /// Plugin configuration class
    /// </summary>
    public class PluginConfiguration : BasePluginConfiguration
    {
        public PluginConfiguration()
        {
            EnableAutoScanning = true;
            ScanIntervalMinutes = 60;
            MinBackdropsForCompletion = 4;
            CacheRefreshHours = 24;
            IsEnabled = true;
        }

        /// <summary>
        /// Enable automatic library scanning
        /// </summary>
        public bool EnableAutoScanning { get; set; }

        /// <summary>
        /// Scan interval in minutes
        /// </summary>
        public int ScanIntervalMinutes { get; set; }

        /// <summary>
        /// Minimum backdrop count to consider item complete
        /// </summary>
        public int MinBackdropsForCompletion { get; set; }

        /// <summary>
        /// Cache refresh interval in hours
        /// </summary>
        public int CacheRefreshHours { get; set; }

        /// <summary>
        /// Plugin enabled state
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Show badges on library items
        /// </summary>
        public bool ShowLibraryBadges { get; set; } = true;

        /// <summary>
        /// Badge style (Percentage, MissingCount, or Simple)
        /// </summary>
        public string BadgeStyle { get; set; } = "Percentage";
    }
}
