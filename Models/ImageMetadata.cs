using System;
using System.Collections.Generic;

namespace Jellyfin.Plugin.ArtisanJelly.Models
{
    /// <summary>
    /// Represents the image status for a single media item
    /// </summary>
    public class ItemImageStatus
    {
        public string ItemId { get; set; }
        public string ItemName { get; set; }
        public string ItemType { get; set; }
        public DateTime LastScanned { get; set; }

        // Tracks what this specific item type actually supports
        public HashSet<string> SupportedImageTypes { get; set; } = new();

        // Removed the hardcoded default types. The scanner will populate this now.
        public Dictionary<string, bool> SingularImages { get; set; } = new();

        public int BackdropCount { get; set; }

        public CompletionStatus GetCompletionStatus()
        {
            int presentCount = 0;
            foreach (var image in SingularImages.Values)
            {
                if (image)
                    presentCount++;
            }

            int totalSingularSupported = SingularImages.Count;

            // Avoid division by zero for items that support 0 singular images
            double percentage =
                totalSingularSupported > 0
                    ? (presentCount / (double)totalSingularSupported) * 100
                    : 100;

            // If it supports backdrops, we expect at least 1
            bool requiresBackdrop = SupportedImageTypes.Contains("Backdrop");
            bool backdropsComplete = !requiresBackdrop || BackdropCount >= 1;

            return new CompletionStatus
            {
                SingularImagesPresent = presentCount,
                SingularImagesMissing = totalSingularSupported - presentCount,
                BackdropCount = BackdropCount,
                IsComplete = presentCount == totalSingularSupported && backdropsComplete,
                CompletionPercentage = percentage,
            };
        }
    }

    public class CompletionStatus
    {
        public int SingularImagesPresent { get; set; }
        public int SingularImagesMissing { get; set; }
        public int BackdropCount { get; set; }
        public bool IsComplete { get; set; }
        public double CompletionPercentage { get; set; }
    }

    /// <summary>
    /// Filter criteria for searching items
    /// </summary>
    public class FilterCriteria
    {
        public string Name { get; set; }
        public string CreatedDate { get; set; }

        // What to search for
        public List<string> MissingImages { get; set; } = new();
        public int? MinBackdrops { get; set; }
        public int? MaxBackdrops { get; set; }
        public string TitleFilter { get; set; }
        public string ItemType { get; set; }
        public bool? IsComplete { get; set; }
    }

    /// <summary>
    /// Result of applying filters
    /// </summary>
    public class FilterResult
    {
        public List<ItemImageStatus> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    /// <summary>
    /// Saved search configuration
    /// </summary>
    public class SavedSearch
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public FilterCriteria Criteria { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ResultCount { get; set; }
    }

    /// <summary>
    /// Plugin statistics
    /// </summary>
    public class ScannerStatistics
    {
        public DateTime LastScanTime { get; set; }
        public int TotalItemsScanned { get; set; }
        public int ItemsWithMissingImages { get; set; }
        public int ItemsComplete { get; set; }
        public int AverageBackdropCount { get; set; }
        public Dictionary<string, int> MissingImageCounts { get; set; }

        // Dynamically calculated list of item types currently in the library
        public string[] AvailableItemTypes { get; set; } = Array.Empty<string>();
    }
}
