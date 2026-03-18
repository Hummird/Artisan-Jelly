using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jellyfin.Plugin.ArtisanJelly.Models;
using Jellyfin.Plugin.ArtisanJelly.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.ArtisanJelly.Controllers
{
    [ApiController]
    [Route("Plugins/ArtisanJelly")]
    [Authorize]
    public class ArtisanJellyController : ControllerBase
    {
        private readonly ArtisanJellyService _scannerService;
        private readonly FilterService _filterService;
        private readonly SearchManager _searchManager;
        private readonly ILogger<ArtisanJellyController> _logger;

        public ArtisanJellyController(
            ArtisanJellyService scannerService,
            FilterService filterService,
            SearchManager searchManager,
            ILogger<ArtisanJellyController> logger
        )
        {
            _scannerService = scannerService;
            _filterService = filterService;
            _searchManager = searchManager;
            _logger = logger;
        }

        [HttpPost("Scan")]
        public async Task<ActionResult<List<ItemImageStatus>>> ScanLibrary(
            [FromQuery] bool forceRefresh = false
        )
        {
            try
            {
                var results = await _scannerService.ScanLibraryAsync(forceRefresh);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scanning library");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("Results")]
        public ActionResult<List<ItemImageStatus>> GetResults() =>
            Ok(_scannerService.GetCachedResults());

        [HttpPost("Filter")]
        public ActionResult<FilterResult> ApplyFilter([FromBody] FilterRequest request)
        {
            try
            {
                var items = _scannerService.GetCachedResults();
                var result = _filterService.ApplyFilters(
                    items,
                    request.Criteria,
                    request.PageNumber,
                    request.PageSize
                );

                if (!string.IsNullOrEmpty(request.SearchId))
                    _searchManager.UpdateSearchResultCount(request.SearchId, result.TotalCount);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying filter");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("Statistics")]
        public ActionResult<ScannerStatistics> GetStatistics()
        {
            var items = _scannerService.GetCachedResults();
            return Ok(_filterService.GetStatistics(items));
        }

        [HttpPost("Search")]
        public ActionResult<SavedSearch> CreateSearch([FromBody] CreateSearchRequest request)
        {
            try
            {
                return Ok(
                    _searchManager.CreateSearch(request.Name, request.Description, request.Criteria)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating search");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("Searches")]
        public ActionResult<List<SavedSearch>> GetSearches() => Ok(_searchManager.GetAllSearches());

        [HttpDelete("Search/{id}")]
        public ActionResult DeleteSearch(string id)
        {
            try
            {
                _searchManager.DeleteSearch(id);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting search");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("Item/{id}")]
        public ActionResult<ItemImageStatus> GetItem(string id)
        {
            var item = _scannerService.GetItemStatus(id);
            return item == null ? NotFound() : Ok(item);
        }

        [HttpGet("UI")]
        [AllowAnonymous] // Anyone can load the HTML, API calls still require auth
        [Produces("text/html")]
        public ActionResult GetUI()
        {
            try
            {
                var assembly = typeof(Plugin).Assembly;
                using var stream = assembly.GetManifestResourceStream(
                    "Jellyfin.Plugin.ArtisanJelly.Web.imagescanner.html"
                );
                if (stream == null)
                    return NotFound("UI resource not found in assembly.");

                using var reader = new System.IO.StreamReader(stream);
                return Content(reader.ReadToEnd(), "text/html");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }

    public class FilterRequest
    {
        public FilterCriteria Criteria { get; set; }
        public string SearchId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }

    public class CreateSearchRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public FilterCriteria Criteria { get; set; }
    }
}
