using EDP_Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace EDP_Backend.Controllers
{
    [ApiController]
    [Route("Admin/Dashboard")]
    public class DashboardAdminController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly IConfiguration _configuration;
        public DashboardAdminController(MyDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

		[SwaggerOperation(Summary = "Get statitics for website")]
		[HttpGet(), Authorize(Roles = "Admin")]
		public IActionResult Dashboard()
        {
            // Get user count
            int userCount = _context.Users.Count();

            // Get activity count
            int activityCount = _context.Activities.Count();

			var firstDay = DateTime.Today.AddDays(-30);

			// Get transaction money 
			decimal transactionMoney = _context.Transactions.Where(t => t.Type == "Gift" || t.Type == "Topup" && t.CreatedAt >= firstDay).Sum(t => t.Amount);

            return Ok(new
            {
				userCount,
				activityCount,
				transactionMoney
			});
        }
	}
}