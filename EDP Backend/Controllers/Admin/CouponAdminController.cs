using EDP_Backend.Models;
using EDP_Backend.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace EDP_Backend.Controllers
{
    [ApiController]
    [Route("Admin/Coupon")]
    public class CouponAdminController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly IConfiguration _configuration;
        public CouponAdminController(MyDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [SwaggerOperation(Summary = "Create a new coupon")]
        [HttpPost(), Authorize(Roles = "Admin")]
        public IActionResult CreateCoupon([FromBody] CreateCouponRequest request)
        {
            string? code = request.Code.Trim();
            string? description = request.Description?.Trim();
            string? discountType = request.DiscountType.Trim();
            decimal discountAmount = request.DiscountAmount;
            DateTime expiry = request.Expiry;

            // Check if code is already registered
            Coupon? existingCoupon = _context.Coupons.FirstOrDefault(coupon => coupon.Code == code);
            if (existingCoupon != null)
            {
                return BadRequest(Helper.Helper.GenerateError("Coupon with this code already exists"));
            }

            // Create coupon
            Coupon coupon = new Coupon
            {
                Code = code,
                Description = description,
                DiscountType = discountType,
                DiscountAmount = discountAmount,
                Expiry = expiry
            };

            _context.Coupons.Add(coupon);
            _context.SaveChanges();
            return Ok(coupon);
        }

        [SwaggerOperation(Summary = "Get all current coupons")]
        [HttpGet(), Authorize(Roles = "Admin")]
        public IActionResult GetCoupons()
        {
            return Ok(_context.Coupons.Where(coupon => !coupon.IsDeleted));
        }


        [SwaggerOperation(Summary = "Get a specific coupon")]
        [HttpGet("{id}"), Authorize(Roles = "Admin")]
        public IActionResult GetCoupon(int id)
        {
            Coupon? coupon = _context.Coupons.Find(id);
            if (coupon == null)
            {
                return NotFound(Helper.Helper.GenerateError("Coupon not found"));
            }
            return Ok(coupon);
        }

        [SwaggerOperation(Summary = "Delete a specific coupon")]
        [HttpDelete("{id}"), Authorize(Roles = "Admin")]
        public IActionResult DeleteCoupon(int id)
        {
            Coupon? coupon = _context.Coupons.Find(id);
            if (coupon == null)
            {
                return NotFound(Helper.Helper.GenerateError("Coupon not found"));
            }
            coupon.IsDeleted = true;
            _context.SaveChanges();
            return Ok(coupon);
        }
    }
}