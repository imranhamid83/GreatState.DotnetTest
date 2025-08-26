using GreatState.DotnetTest.API.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace GreatState.DotnetTest.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PagesController : ControllerBase
    {
        private readonly CMSPages _dbContext;
        private readonly IMemoryCache _cache;



        public PagesController (CMSPages dbContext, IMemoryCache cache)
        {
            _dbContext = dbContext;
            _cache = cache;
        }

        /// <summary>
        /// View a page with a specific role and a key
        /// </summary>
        [HttpGet]
        /* You can assign a role to an API key in Sitecore which then be configured to perform specific tasks; this approach will help streamline
        the role based access.
        */
        public IActionResult GetPages([FromQuery] string role, string key, bool useCache = true)
        {
            string cacheKey = $"pages_{role}_{key}";

            try
            {
                if (useCache && _cache.TryGetValue(cacheKey, out object cached))
                    return Ok(new { cached = true, timestamp = DateTime.UtcNow, data = cached });

                if (!IsAuthorized(role, key))
                    return BadRequest(new { message = "Invalid role or key", timestamp = DateTime.UtcNow });

                var visiblePages = _dbContext.Pages.Where(p => p.RoleRequired == role).ToList();

                if (useCache)
                {
                    _cache.Set(cacheKey, visiblePages, TimeSpan.FromMinutes(5)); // cache expires in 5 min
                }

                return Ok(new { cached = false, timestamp = DateTime.UtcNow, data = visiblePages });
            }
            catch (Exception ex)
            {
                if (_cache.TryGetValue(cacheKey, out object cached))
                {
                    return StatusCode(503, new { message = "CMS unavailable, serving cached data", timestamp = DateTime.UtcNow, data = cached });
                }

                return StatusCode(500, new { message = "CMS is unavailable", error = ex.Message, timestamp = DateTime.UtcNow });
            }
        }

        /// <summary>
        /// Create a new page (Admin/Staff only)
        /// </summary>
        [HttpPost]
        public IActionResult CreatePage([FromQuery] string role, string key, [FromBody] Page page)
        {
            try
            {
                if (!IsAuthorizedforCRUD(role, key))
                    return Unauthorized(new { message = "You are not authorized to create pages", timestamp = DateTime.UtcNow });

                _dbContext.Pages.Add(page);
                _dbContext.SaveChanges();

                _cache.Remove("pages_all"); // invalidate cache
                return Ok(new { message = "Page created successfully", page, timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {                
                return StatusCode(500, new { message = "CMS is unavailable", error = ex.Message, timestamp = DateTime.UtcNow });
            }
        }

        /// <summary>
        /// Update an existing page (Admin/Staff only)
        /// </summary>
        [HttpPut("{id}")]
        public IActionResult UpdatePage(int id, [FromQuery] string role, string key, [FromBody] Page updatedPage)
        {
            try
            {
                if (!IsAuthorizedforCRUD(role, key))
                    return Unauthorized(new { message = "Only admin or staff can update pages", timestamp = DateTime.UtcNow });

                var page = _dbContext.Pages.FirstOrDefault(p => p.Id == id);
                if (page == null)
                    return NotFound(new { message = "Page not found", timestamp = DateTime.UtcNow });

                page.Title = updatedPage.Title;
                page.Body = updatedPage.Body;
                page.RoleRequired = updatedPage.RoleRequired;

                _dbContext.SaveChanges();
                _cache.Remove("pages_all"); // invalidate cache

                return Ok(new { message = "Page updated successfully", page, timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "CMS is unavailable", error = ex.Message, timestamp = DateTime.UtcNow });
            }

        }

        private bool IsAuthorized(string role, string key)
        {
            var matchKey = _dbContext.Roles.FirstOrDefault(r => r.Key == key && r.Name == role);
            if (matchKey == null) return false;

            var roleName = matchKey.Name.ToLower();
            return roleName.Contains("admin") || roleName.Contains("staff") || roleName.Contains("anonymous");
        }

        private bool IsAuthorizedforCRUD(string role, string key)
        {
            var matchKey = _dbContext.Roles.FirstOrDefault(r => r.Key == key && r.Name == role);
            if (matchKey == null) return false;

            var roleName = matchKey.Name.ToLower();
            return roleName.Contains("admin") || roleName.Contains("staff");
        }

    }
}
