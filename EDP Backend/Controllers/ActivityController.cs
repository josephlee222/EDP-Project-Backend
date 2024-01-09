using EDP_Backend.Models;
using EDP_Backend.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;


//TODO: delete routes that customers shouldn't edit


namespace EDP_Backend.Controllers
{
    [ApiController]
    [Route("/Activity")]
    public class ActivityController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly IConfiguration _configuration;

        public ActivityController(MyDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [SwaggerOperation(Summary = "Get all current activities")]
        [HttpGet(), /*Authorize(Roles = "Admin")*/]
        public IActionResult GetActivities()
        {
            return Ok(_context.Activities);
        }


        [SwaggerOperation(Summary = "Get a specific activity")]
        [HttpGet("{id}")]
        public IActionResult Getactivity(int id)
        {
            Activity? activity = _context.Activities.Find(id);
            if (activity == null)
            {
                return NotFound(Helper.Helper.GenerateError("activity not found"));
            }
            return Ok(activity);
        }
    }
}
