using System.Security.Claims;
using EDP_Backend.Hubs;
using EDP_Backend.Models;
using EDP_Backend.Models.Admin;
using Google.Apis.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
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
		private readonly IHubContext<ActionsHub> _hubContext;

		public ShopController(MyDbContext context, IConfiguration configuration, IHubContext<ActionsHub> hubContext)
        {
            _context = context;
            _configuration = configuration;
            _hubContext = hubContext;
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
            var tax = 0.09;
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

		[SwaggerOperation(Summary = "Delete cart item")]
		[HttpDelete("Cart"), Authorize]
        public IActionResult DeleteCartItem([FromQuery] int id)
        {
            // Get user id
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value);

			// Get cart item
			var cartItem = _context.Carts.FirstOrDefault(c => c.Id == id && c.User.Id == userId);

			// Check if cart item exists
			if (cartItem == null)
            {
				return NotFound(Helper.Helper.GenerateError("Cart item not found"));
			}

			// Remove cart item
			_context.Carts.Remove(cartItem);
			_context.SaveChanges();
			_hubContext.Clients.Groups(userId.ToString()).SendAsync("refresh");

			// Return success
			return Ok();
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

		[SwaggerOperation(Summary = "Finalise checkout")]
		[HttpPost("Cart/Checkout"), Authorize]
		public IActionResult Checkout([FromBody] CheckoutRequest request)
        {
            // Get user id
            int id = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value);

            // Get user
            var user = _context.Users.Find(id);

            // Get all cart items
            var cart = _context.Carts.Include(c => c.Availability).Where(c => c.User.Id == id).ToList();

            // Get total price
            var totalPrice = 0.0;
            var tax = 0.09;
            foreach (var item in cart)
            {
				totalPrice += item.Availability.Price * item.Pax;
			}
            // Round to 2 decimal places
			totalPrice = Math.Round(totalPrice, 2);
			var subTotal = totalPrice;
			var taxTotal = totalPrice * tax;
			totalPrice += (totalPrice * tax);

			// Check if coupon exists
			if (request.Coupon != null)
            {
                // Get coupon
                var coupon = _context.Coupons.Find(request.Coupon);

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
                // Apply coupon
				totalPrice -= (double)coupon.DiscountAmount;
			}

			// Create a booking for each cart item
            foreach (var item in cart)
            {
                // Create booking
                var booking = new Booking
                {
					UserId = user.Id,
					Availability = item.Availability,
					Pax = item.Pax,
					Date = item.Availability.Date,
                    Name = request.Name,
                    Email = request.Email,
                    Phone = request.Phone,
                    Birthday = request.Birthday,
                    Nric = request.Nric,
				};

                // Add booking to database
                _context.Bookings.Add(booking);
            }

            if (user.Balance < (decimal)totalPrice)
            {
                return BadRequest(Helper.Helper.GenerateError("Insufficient balance"));
            }

            // Deduct from user's wallet
            user.Balance -= (decimal)totalPrice;

            // Add transaction
            var transaction = new Models.Transaction
            {
                User = user,
				Amount = (decimal)totalPrice,
				Type = "Activity Booking",
                Settled = true,
			};

            // Add transaction
            _context.Transactions.Add(transaction);


			// Remove cart items
			_context.Carts.RemoveRange(cart);
			_context.SaveChanges();
			_hubContext.Clients.Groups(user.Id.ToString()).SendAsync("refresh");

			return Ok();
        }
	}
}
