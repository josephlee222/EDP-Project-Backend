using EDP_Backend.Models;
using EDP_Backend.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;


namespace EDP_Backend.Controllers.Admin
{
    [ApiController]
    [Route("Admin/Availability")]
    public class AvailabilityAdminController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly IConfiguration _configuration;

        public AvailabilityAdminController(MyDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [SwaggerOperation(Summary = "Get all current availabilities")]
        [HttpGet(), /*Authorize(Roles = "Admin")*/]
        public IActionResult GetAvailabilities()
        {
            return Ok(_context.Availabilities);
        }


        [SwaggerOperation(Summary = "Create a new availability")]
        [HttpPost()]
        public IActionResult CreateAvailability([FromBody] CreateAvailabilityRequest request)
        {
            int activityId = request.ActivityId;
            DateTime date = request.Date;
            float price = request.Price;
            int maxpax = request.MaxPax;
            int currentpax = request.CurrentPax;

            // Check if name is already registered
            Availability? existingAvailability = _context.Availabilities.Include(a => a.Activity).FirstOrDefault(availability => 
            availability.Activity.Id == activityId && availability.Date == date);
            if (existingAvailability != null)
            {
                return BadRequest(Helper.Helper.GenerateError("Availability with this name already exists"));
            }

            // get activity
            Activity? activity = _context.Activities.Find(activityId);

            if (activity == null)
            {
				return NotFound(Helper.Helper.GenerateError("activity not found"));
			}

            // Create availability
            Availability availability = new Availability
            {
                Activity = activity,
                Date = date,
                Price = price,
                MaxPax = maxpax,
                CurrentPax = currentpax,
            };

            _context.Availabilities.Add(availability);
            _context.SaveChanges();
            return Ok(availability);
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
            var activityAvailabilities = _context.Availabilities.Include(a => a.Activity).Include(a => a.Bookings) // Include activity
                                           .Where(a => a.Activity.Id == id)
                                           .ToList(); // Filter and convert to list

            if (activityAvailabilities == null)
            {
                return NotFound(Helper.Helper.GenerateError("availability not found for activity with id "+id.ToString()));
            }
            return Ok(activityAvailabilities);
        }


        [SwaggerOperation(Summary = "Update a specific availability")]
        [HttpPut("{id}")]
        public IActionResult Editavailability(int id, [FromBody] EditAvailabilityRequest request)
        {
            // Get availability
            Availability? availability = _context.Availabilities.Find(id);

            // Check if availability exists
            if (availability == null)
            {
                return NotFound(Helper.Helper.GenerateError("availability not found"));
            }

            // Update availability

            int activityId = request.ActivityId;
            DateTime date = request.Date;
            float? price = request.Price;
            int? maxpax = request.MaxPax;
            int? currentpax = request.CurrentPax;

            //availability.Code = code ?? availability.Code;

            // get activity
            Activity? activity = _context.Activities.Find(activityId);
            if (activity == null)
            {
                return NotFound(Helper.Helper.GenerateError("activity not found"));
            }

            availability.Activity = activity;
            availability.Date = date;
            availability.Price = price ?? availability.Price;
            availability.MaxPax = maxpax ?? availability.MaxPax;
            availability.CurrentPax = currentpax ?? availability.CurrentPax;

            _context.SaveChanges();
            return Ok(availability);
        }

        [SwaggerOperation(Summary = "Delete a specific availability")]
        [HttpDelete("{id}")]
        public IActionResult Deleteavailability(int id)
        {
            Availability? availability = _context.Availabilities.Find(id);
            if (availability == null)
            {
                return NotFound(Helper.Helper.GenerateError("availability not found"));
            }
            _context.Availabilities.Remove(availability);
            _context.SaveChanges();
            return Ok(availability);
        }
    }
}
