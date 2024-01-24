using EDP_Backend.Models;
using EDP_Backend.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;





namespace EDP_Backend.Controllers
{
    [ApiController]
    [Route("/Friend")]
    public class FriendController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly IConfiguration _configuration;

        public FriendController(MyDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [SwaggerOperation(Summary = "Get all current friends")]
        [HttpGet(), Authorize]
        public IActionResult GetActivities()
        {
            return Ok(_context.Activities);
        }


        [SwaggerOperation(Summary = "Get a specific activity")]
        [HttpGet("{id}"), Authorize]
        public IActionResult Getactivity(int id)
        {
            Activity? activity = _context.Activities.Find(id);
            if (activity == null)
            {
                return NotFound(Helper.Helper.GenerateError("Friend not found"));
            }
            return Ok(activity);
        }
    }
}
