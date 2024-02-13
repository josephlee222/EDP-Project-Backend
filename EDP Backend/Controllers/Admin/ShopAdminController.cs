using EDP_Backend.Models;
using EDP_Backend.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;


namespace EDP_Backend.Controllers.Admin
{
    [ApiController]
    [Route("Admin/Shop")]
    public class ShopAdminController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly IConfiguration _configuration;

        public ShopAdminController(MyDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [SwaggerOperation(Summary = "Get all current banners")]
        [HttpGet("Banner"), Authorize(Roles = "Admin")]
        public IActionResult GetBanners()
        {
            // Get banner 1
            var banner1 = _context.Banners.Where(b => b.Active).OrderByDescending(b => b.Id).FirstOrDefault(b => b.Slot == 1);
            // Get banner 2
            var banner2 = _context.Banners.Where(b => b.Active).OrderByDescending(b => b.Id).FirstOrDefault(b => b.Slot == 2);
            // Get banner 3
            var banner3 = _context.Banners.Where(b => b.Active).OrderByDescending(b => b.Id).FirstOrDefault(b => b.Slot == 3);
            // Get banner 4
            var banner4 = _context.Banners.Where(b => b.Active).OrderByDescending(b => b.Id).FirstOrDefault(b => b.Slot == 4);

            // Return all banners
            return Ok(new
            {
				banner1,
				banner2,
				banner3,
				banner4
			});
        }

		[SwaggerOperation(Summary = "Create a new banner")]
		[HttpPost("Banner"), Authorize(Roles = "Admin")]
		public IActionResult PostBanner([FromBody] PostBannerRequest request)
		{
			var banner = new Banner
            {
				ImagePath = request.ImagePath,
				Slot = request.Slot,
			};

            var existingBanner = _context.Banners.OrderByDescending(b => b.Id).FirstOrDefault(b => b.Slot == request.Slot);
            if (existingBanner != null)
            {
				existingBanner.Active = false;
				_context.Banners.Update(existingBanner);
			}

            _context.Banners.Add(banner);
            _context.SaveChanges();

            return Ok(banner);
		}





	}
}
