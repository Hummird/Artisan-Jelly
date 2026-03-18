using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jellyfin.Data.Enums;
using Jellyfin.Plugin.ArtisanJelly.Models;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.ArtisanJelly.Services
{
    public class ArtisanJellyService
    {
        private readonly ILibraryManager _libraryManager;
        private readonly ILogger<ArtisanJellyService> _logger;
        private List<ItemImageStatus> _scanCache = new();
        private DateTime _lastScanTime = DateTime.MinValue;

        // Maps friendly display names -> actual Jellyfin ImageType enum values
        private static readonly Dictionary<string, ImageType> ImageTypeMap = new()
        {
            { "Primary", ImageType.Primary },
            { "Clearart", ImageType.Art },
            { "Banner", ImageType.Banner },
            { "BoxRear", ImageType.BoxRear },
            { "Disc", ImageType.Disc },
            { "Logo", ImageType.Logo },
            { "Thumb", ImageType.Thumb },
        };

        public ArtisanJellyService(
            ILibraryManager libraryManager,
            ILogger<ArtisanJellyService> logger
        )
        {
            _libraryManager = libraryManager;
            _logger = logger;
        }

        public async Task<List<ItemImageStatus>> ScanLibraryAsync(bool forceRefresh = false)
        {
            if (!forceRefresh && (DateTime.UtcNow - _lastScanTime).TotalMinutes < 5)
            {
                _logger.LogInformation("Returning cached scan results");
                return _scanCache;
            }

            _logger.LogInformation("Starting library image scan");
            var results = new List<ItemImageStatus>();

            var query = new InternalItemsQuery
            {
                Recursive = true,
                // Use BaseItemKind enum — NOT strings
                IncludeItemTypes = new[] { BaseItemKind.Movie, BaseItemKind.Series },
            };

            var queryResult = _libraryManager.GetItemsResult(query);

            foreach (var item in queryResult.Items)
            {
                results.Add(ScanItem(item));
            }

            _scanCache = results;
            _lastScanTime = DateTime.UtcNow;
            _logger.LogInformation("Scan complete: {Count} items", results.Count);

            return await Task.FromResult(results);
        }

        private ItemImageStatus ScanItem(BaseItem item)
        {
            var status = new ItemImageStatus
            {
                ItemId = item.Id.ToString(),
                ItemName = item.Name,
                ItemType = item is Movie ? "Movie" : "Series",
                LastScanned = DateTime.UtcNow,
            };

            // Use ImageInfos (always available on BaseItem) — no GetImagePath needed
            foreach (var kvp in ImageTypeMap)
            {
                status.SingularImages[kvp.Key] =
                    item.ImageInfos != null && item.ImageInfos.Any(img => img.Type == kvp.Value);
            }

            // Count backdrops via ImageInfos
            status.BackdropCount =
                item.ImageInfos != null
                    ? item.ImageInfos.Count(img => img.Type == ImageType.Backdrop)
                    : 0;

            return status;
        }

        public ItemImageStatus GetItemStatus(string itemId)
        {
            return _scanCache.FirstOrDefault(x => x.ItemId == itemId);
        }

        public List<ItemImageStatus> GetCachedResults() => _scanCache;
    }
}
