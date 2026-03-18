using System;
using System.Collections.Generic;
using System.Linq;
using Jellyfin.Plugin.ArtisanJelly.Models;

namespace Jellyfin.Plugin.ArtisanJelly.Services
{
    /// <summary>
    /// Service for filtering scan results
    /// </summary>
    public class FilterService
    {
        /// <summary>
        /// Apply filters to scan results
        /// </summary>
        public FilterResult ApplyFilters(
            List<ItemImageStatus> items,
            FilterCriteria criteria,
            int pageNumber = 1,
            int pageSize = 50
        )
        {
            var filtered = items.AsEnumerable();

            // Filter by item type
            if (!string.IsNullOrEmpty(criteria.ItemType) && criteria.ItemType != "All")
            {
                filtered = filtered.Where(x => x.ItemType == criteria.ItemType);
            }

            // Filter by title
            if (!string.IsNullOrEmpty(criteria.TitleFilter))
            {
                var search = criteria.TitleFilter.ToLower();
                // null check to prevent crashes
                filtered = filtered.Where(x =>
                    x.ItemName != null && x.ItemName.ToLower().Contains(search)
                );
            }

            // Filter by missing images
            if (criteria.MissingImages?.Count > 0)
            {
                filtered = filtered.Where(item =>
                    criteria.MissingImages.Any(img =>
                        !item.SingularImages.GetValueOrDefault(img, false)
                    )
                );
            }

            // Filter by backdrop count
            if (criteria.MinBackdrops.HasValue)
            {
                filtered = filtered.Where(x => x.BackdropCount < criteria.MinBackdrops.Value);
            }

            if (criteria.MaxBackdrops.HasValue)
            {
                filtered = filtered.Where(x => x.BackdropCount <= criteria.MaxBackdrops.Value);
            }

            // Filter by completion status
            if (criteria.IsComplete.HasValue)
            {
                filtered = filtered.Where(x =>
                    x.GetCompletionStatus().IsComplete == criteria.IsComplete.Value
                );
            }

            var totalCount = filtered.Count();
            var skip = (pageNumber - 1) * pageSize;
            var pagedItems = filtered.Skip(skip).Take(pageSize).ToList();

            return new FilterResult
            {
                Items = pagedItems,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
            };
        }

        /// <summary>
        /// Get statistics about the current items
        /// </summary>
        public ScannerStatistics GetStatistics(List<ItemImageStatus> items)
        {
            var stats = new ScannerStatistics
            {
                LastScanTime = DateTime.UtcNow,
                TotalItemsScanned = items.Count,
                MissingImageCounts = new(),
            };

            // Calculate distinct item types dynamically
            stats.AvailableItemTypes = items
                .Where(x => !string.IsNullOrWhiteSpace(x.ItemType))
                .Select(x => x.ItemType)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToArray();

            var completeCount = 0;
            var missingCount = 0;
            int totalBackdrops = 0;

            var imageTypes = new[]
            {
                "Primary",
                "Clearart",
                "Banner",
                "BoxRear",
                "Disc",
                "Logo",
                "Thumb",
            };
            foreach (var imageType in imageTypes)
            {
                stats.MissingImageCounts[imageType] = 0;
            }

            foreach (var item in items)
            {
                var completion = item.GetCompletionStatus();

                if (completion.IsComplete)
                    completeCount++;
                else
                    missingCount++;

                totalBackdrops += item.BackdropCount;

                // Count missing images
                foreach (var imageType in imageTypes)
                {
                    if (!item.SingularImages.GetValueOrDefault(imageType, false))
                    {
                        stats.MissingImageCounts[imageType]++;
                    }
                }
            }

            stats.ItemsComplete = completeCount;
            stats.ItemsWithMissingImages = missingCount;
            stats.AverageBackdropCount = items.Count > 0 ? totalBackdrops / items.Count : 0;

            return stats;
        }
    }
}
