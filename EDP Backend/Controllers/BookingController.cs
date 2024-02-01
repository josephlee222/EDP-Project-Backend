using EDP_Backend.Models;
using EDP_Backend.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;


namespace EDP_Backend.Controllers.Admin
{
    [ApiController]
    [Route("/Booking")]
    public class BookingController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly IConfiguration _configuration;

        public BookingController(MyDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [SwaggerOperation(Summary = "Get all current bookings")]
        [HttpGet(), Authorize(Roles = "Admin")]
        public IActionResult GetBookings()
        {
            return Ok(_context.Bookings);
        }


        [SwaggerOperation(Summary = "Create a new booking")]
        [HttpPost(), Authorize(Roles = "Admin")]
        public IActionResult CreateBooking([FromBody] CreateBookingRequest request)
        {
            int userId = request.UserId;
            int activityId = request.ActivityId;
            DateTime date = request.Date;
            int pax = request.Pax;
            string? notes = request.Notes;

            // Check if name is already registered
            Booking? existingBooking = _context.Bookings.FirstOrDefault(booking =>
            booking.UserId == userId && booking.ActivityId == activityId && booking.Date == date);
            if (existingBooking != null)
            {
                return BadRequest(Helper.Helper.GenerateError("Booking with this name already exists"));
            }

            // Create booking
            Booking booking = new Booking
            {
                UserId = userId,
                ActivityId = activityId,
                Date = date,
                Pax = pax,
                Notes = notes,
            };

            _context.Bookings.Add(booking);
            _context.SaveChanges();
            return Ok(booking);
        }


        [SwaggerOperation(Summary = "Get a specific booking")]
        [HttpGet("{id}"), Authorize(Roles = "Admin")]
        public IActionResult Getbooking(int id)
        {
            Booking? booking = _context.Bookings.Find(id);
            if (booking == null)
            {
                return NotFound(Helper.Helper.GenerateError("booking not found"));
            }
            return Ok(booking);
        }

        [SwaggerOperation(Summary = "Update a specific booking")]
        [HttpPut("{id}"), Authorize(Roles = "Admin")]
        public IActionResult Editbooking(int id, [FromBody] EditBookingRequest request)
        {
            // Get booking
            Booking? booking = _context.Bookings.Find(id);

            // Check if booking exists
            if (booking == null)
            {
                return NotFound(Helper.Helper.GenerateError("booking not found"));
            }

            // Update booking

            int userId = request.UserId;
            int activityId = request.ActivityId;
            DateTime date = request.Date;
            int pax = request.Pax;
            string? notes = request.Notes;

            booking.UserId = userId;
            booking.ActivityId = activityId;
            booking.Date = date;
            booking.Pax = pax;
            booking.Notes = notes;

            _context.SaveChanges();
            return Ok(booking);
        }

        [SwaggerOperation(Summary = "Delete a specific booking")]
        [HttpDelete("{id}"), Authorize(Roles = "Admin")]
        public IActionResult Deletebooking(int id)
        {
            Booking? booking = _context.Bookings.Find(id);
            if (booking == null)
            {
                return NotFound(Helper.Helper.GenerateError("booking not found"));
            }
            _context.Bookings.Remove(booking);
            _context.SaveChanges();
            return Ok(booking);
        }


    }
}

