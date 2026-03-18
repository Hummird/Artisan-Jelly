using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Jellyfin.Plugin.ArtisanJelly.Models;
using MediaBrowser.Common.Configuration;

namespace Jellyfin.Plugin.ArtisanJelly.Services
{
    /// <summary>
    /// Manage saved searches/filters
    /// </summary>
    public class SearchManager
    {
        private readonly IApplicationPaths _applicationPaths;
        private readonly string _searchFile;
        private List<SavedSearch> _searches;

        public SearchManager(IApplicationPaths applicationPaths)
        {
            _applicationPaths = applicationPaths;
            var pluginDir = Path.Combine(applicationPaths.PluginConfigurationsPath, "ArtisanJelly");
            Directory.CreateDirectory(pluginDir);
            _searchFile = Path.Combine(pluginDir, "savedSearches.json");
            LoadSearches();
        }

        /// <summary>
        /// Load searches from file
        /// </summary>
        private void LoadSearches()
        {
            if (File.Exists(_searchFile))
            {
                try
                {
                    var json = File.ReadAllText(_searchFile);
                    _searches =
                        JsonSerializer.Deserialize<List<SavedSearch>>(json)
                        ?? new List<SavedSearch>();
                }
                catch
                {
                    _searches = new List<SavedSearch>();
                }
            }
            else
            {
                _searches = new List<SavedSearch>();
            }
        }

        /// <summary>
        /// Save searches to file
        /// </summary>
        private void SaveSearches()
        {
            var json = JsonSerializer.Serialize(
                _searches,
                new JsonSerializerOptions { WriteIndented = true }
            );
            File.WriteAllText(_searchFile, json);
        }

        /// <summary>
        /// Create a new search
        /// </summary>
        public SavedSearch CreateSearch(string name, string description, FilterCriteria criteria)
        {
            var search = new SavedSearch
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                Description = description,
                Criteria = criteria,
                CreatedDate = DateTime.UtcNow,
                ResultCount = 0,
            };

            _searches.Add(search);
            SaveSearches();
            return search;
        }

        /// <summary>
        /// Get all searches
        /// </summary>
        public List<SavedSearch> GetAllSearches() => _searches.ToList();

        /// <summary>
        /// Get search by ID
        /// </summary>
        public SavedSearch GetSearch(string id) => _searches.FirstOrDefault(x => x.Id == id);

        /// <summary>
        /// Delete a search
        /// </summary>
        public void DeleteSearch(string id)
        {
            _searches.RemoveAll(x => x.Id == id);
            SaveSearches();
        }

        /// <summary>
        /// Update a search
        /// </summary>
        public void UpdateSearch(SavedSearch search)
        {
            var existing = _searches.FirstOrDefault(x => x.Id == search.Id);
            if (existing != null)
            {
                var index = _searches.IndexOf(existing);
                _searches[index] = search;
                SaveSearches();
            }
        }

        /// <summary>
        /// Update result count for a search
        /// </summary>
        public void UpdateSearchResultCount(string searchId, int count)
        {
            var search = GetSearch(searchId);
            if (search != null)
            {
                search.ResultCount = count;
                UpdateSearch(search);
            }
        }
    }
}
