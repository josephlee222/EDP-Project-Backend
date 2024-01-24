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
        public IActionResult GetFriends()
        {
            return Ok(_context.Friends);
        }


        [SwaggerOperation(Summary = "Get a specific friend")]
        [HttpGet("{id}"), Authorize]
        public IActionResult GetFriend(int id)
        {
            Friend? Friend = _context.Friends.Find(id);
            if (Friend == null)
            {
                return NotFound(Helper.Helper.GenerateError("Friend not found"));
            }
            return Ok(Friend);
        }
    }
}
