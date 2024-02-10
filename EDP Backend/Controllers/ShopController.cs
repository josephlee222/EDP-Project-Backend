using System.Security.Claims;
using EDP_Backend.Models;
using EDP_Backend.Models.Admin;
using Google.Apis.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;


namespace EDP_Backend.Controllers.Admin
{
    [ApiController]
    [Route("Shop")]
    public class ShopController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly IConfiguration _configuration;

        public ShopController(MyDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [SwaggerOperation(Summary = "Get all current banners")]
        [HttpGet("Banner")]
        public IActionResult GetBanners()
        {
            // Get banner 1
            var banner1 = _context.Banners.Where(b => b.Active).FirstOrDefault(b => b.Slot == 1);
            // Get banner 2
            var banner2 = _context.Banners.Where(b => b.Active).FirstOrDefault(b => b.Slot == 2);
            // Get banner 3
            var banner3 = _context.Banners.Where(b => b.Active).FirstOrDefault(b => b.Slot == 3);
            // Get banner 4
            var banner4 = _context.Banners.Where(b => b.Active).FirstOrDefault(b => b.Slot == 4);

            // Return all banners
            return Ok(new
            {
				banner1,
				banner2,
				banner3,
				banner4
			});
        }


		[SwaggerOperation(Summary = "Create new cart item with avaliablity ID and pax")]
		[HttpPost("Cart"), Authorize]
		public IActionResult CreateCartItem([FromBody] CreateCartItemRequest request)
        {
            // Get user id
            int id = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value);

            // Check if the availability exists
            var availability = _context.Availabilities.Include(a => a.Bookings).FirstOrDefault(a => a.Id == request.AvailabilityId);

            if (availability == null)
            {
				return NotFound(Helper.Helper.GenerateError("Availablity not found"));
			}

            // Check if the availability is full
            var bookedPax = availability.Bookings.Sum(b => b.Pax);
            if (bookedPax + request.Pax > availability.MaxPax)
            {
                return BadRequest(Helper.Helper.GenerateError("The available timeslot is full"));
            }

            // get user
            var user = _context.Users.Find(id);

            // Check if the user already has a cart item for this availability
            var existingCartItem = _context.Carts.Include(c => c.User).FirstOrDefault(c => c.Availability.Id == request.AvailabilityId && c.User.Id == id);

            if (existingCartItem != null)
            {
				// Update cart item
                existingCartItem.Pax += request.Pax;
                _context.SaveChanges();
                return Ok(existingCartItem);
			}
            else
            {
				// Create cart item
				var cartItem = new Cart
				{
					User = user,
					Availability = availability,
					Pax = request.Pax
				};

				// Add cart item to database
				_context.Carts.Add(cartItem);
				_context.SaveChanges();
				return Ok(cartItem);
			}
        }

		[SwaggerOperation(Summary = "Get all cart items")]
		[HttpGet("Cart"), Authorize]
        public IActionResult GetCartItems()
        {
			// Get user id
			int id = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value);

			// Get all cart items
			var cart = _context.Carts.Include(c => c.Availability).ThenInclude(c => c.Activity).Where(c => c.User.Id == id).ToList();

            // Get total price
            var totalPrice = 0.0;
            var tax = 0.07;
            foreach (var item in cart)
            {
                totalPrice += item.Availability.Price * item.Pax;
            }

            // Round to 2 decimal places
            totalPrice = Math.Round(totalPrice, 2);
            var subTotal = totalPrice;
            var taxTotal = totalPrice * tax;
            totalPrice += (totalPrice * tax);
            

			return Ok(new {cart, subTotal, totalPrice, taxTotal});;
		}

		[SwaggerOperation(Summary = "Check whether coupon is valid")]
		[HttpGet("Cart/Coupon"), Authorize]
        public IActionResult CheckCoupon([FromQuery] string code)
        {
			// Get coupon
			var coupon = _context.Coupons.FirstOrDefault(c => c.Code == code);

			// Check if coupon exists
			if (coupon == null)
            {
				return NotFound(Helper.Helper.GenerateError("Coupon not found"));
			}

			// Check if coupon is expired
			if (coupon.Expiry < DateTime.Now)
            {
				return BadRequest(Helper.Helper.GenerateError("Coupon has expired"));
			}

			// Return coupon
			return Ok(coupon);
		}
	}
}
