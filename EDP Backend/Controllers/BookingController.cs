using EDP_Backend.Models;
using EDP_Backend.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
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
        [HttpGet(), Authorize]
        public IActionResult GetBookings()
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value);
            var booking = _context.Bookings.Where(x => x.UserId == userId);
            return Ok(booking);
        }

        // CREATION OF BOOKINGS FOR USER WILL BE HANDLED BY THE CART (CART CONTROLLER)
        // The activityId will change to availabilityId

        //[SwaggerOperation(Summary = "Create a new booking")]
        //[HttpPost(), Authorize]
        //public IActionResult CreateBooking([FromBody] CreateBookingRequest request)
        //{
        //    int id = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value);
        //    User? user = _context.Users.Include(user => user.Notifications).AsNoTracking().FirstOrDefault(user => user.Id == id);

        //    if (user == null)
        //    {
        //        return BadRequest(Helper.Helper.GenerateError("Invalid token"));
        //    }

        //    int userId = id;
        //    int activityId = request.ActivityId;
        //    DateTime date = request.Date;
        //    int pax = request.Pax;
        //    string? notes = request.Notes;

        //    // Check if name is already registered
        //    Booking? existingBooking = _context.Bookings.FirstOrDefault(booking =>
        //    booking.UserId == userId && booking.ActivityId == activityId && booking.Date == date);
        //    if (existingBooking != null)
        //    {
        //        return BadRequest(Helper.Helper.GenerateError("Booking with this name already exists"));
        //    }
        //    if (notes == null)
        //    {
        //        notes = "";
        //    }

        //    // Create booking
        //    Booking booking = new Booking
        //    {
        //        UserId = userId,
        //        ActivityId = activityId,
        //        Date = date,
        //        Pax = pax,
        //        Notes = notes,
        //    };

        //    _context.Bookings.Add(booking);
        //    _context.SaveChanges();
        //    return Ok(booking);
        //}


        [SwaggerOperation(Summary = "Get a specific booking")]
        [HttpGet("{id}"), Authorize]
        public IActionResult Getbooking(int id)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value);

            Booking? booking = _context.Bookings.Where(x => x.UserId == userId).FirstOrDefault(x => x.Id == id);
            if (booking == null)
            {
                return NotFound(Helper.Helper.GenerateError("booking not found"));
            }
            return Ok(booking);
        }

        //[SwaggerOperation(Summary = "Update a specific booking")]
        //[HttpPut("{id}"), Authorize]
        //public IActionResult Editbooking(int id, [FromBody] EditBookingRequest request)
        //{

        //    int UserId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value);

        //    Booking? booking = _context.Bookings.Where(x => x.UserId == UserId).FirstOrDefault(x => x.Id == id);

        //    // Check if booking exists
        //    if (booking == null)
        //    {
        //        return NotFound(Helper.Helper.GenerateError("booking not found"));
        //    }


        //    // Update booking

        //    DateTime date = request.Date;
        //    int pax = request.Pax;
        //    string? notes = request.Notes;

        //    if(notes == null)
        //    {
        //        notes = "";
        //    }

        //    booking.Date = date;
        //    booking.Pax = pax;
        //    booking.Notes = notes;

        //    _context.SaveChanges();
        //    return Ok(booking);
        //}

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

