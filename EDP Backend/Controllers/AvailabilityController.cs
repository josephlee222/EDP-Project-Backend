using EDP_Backend.Models;
using EDP_Backend.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace EDP_Backend.Controllers
{
    [ApiController]
    [Route("/Availability")]
    public class AvailabilityController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly IConfiguration _configuration;

        public AvailabilityController(MyDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [SwaggerOperation(Summary = "Get all current availabilities")]
        [HttpGet()]
        public IActionResult GetAvailabilities()
        {
            return Ok(_context.Availabilities);
        }


        [SwaggerOperation(Summary = "Get a specific availability")]
        [HttpGet("{id}")]
        public IActionResult Getavailability(int id)
        {
            Availability? availability = _context.Availabilities.Find(id);
            if (availability == null)
            {
                return NotFound(Helper.Helper.GenerateError("availability not found"));
            }
            return Ok(availability);
        }

        [SwaggerOperation(Summary = "Get availabilities for a specific activity")]
        [HttpGet("Activity/{id}")]
        public IActionResult GetActivityAvailabilities(int id)
        {

            var availability = _context.Availabilities.Find(id);
            var activityAvailabilities = _context.Availabilities.Include(a => a.Activity).Include(a => a.Bookings)
                                           .Where(a => a.Activity.Id == id)
                                           .ToList(); // Filter and convert to list

            if (activityAvailabilities == null)
            {
                return NotFound(Helper.Helper.GenerateError("availability not found for activity with id " + id.ToString()));
            }
            return Ok(activityAvailabilities);
        }
    }
}
