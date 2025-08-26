using GreatState.DotnetTest.API.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GreatState.DotnetTest.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PagesController : ControllerBase
    {
        private readonly CMSPages _dbContext;

        
        public PagesController (CMSPages dbContext)
        {
            _dbContext = dbContext;
        }
        [HttpGet]
        public IActionResult GetPages([FromQuery] string role , string key)               
        {                
            var matchKey = _dbContext.Roles.Where(r=> r.Key == key && r.Name == role);
            if (matchKey.Any())
            {
                var visiblePages = _dbContext.Pages.Where(p => p.RoleRequired == role).ToList();

                return Ok(visiblePages);
            }
            else
                return BadRequest();
        }
    }
}
