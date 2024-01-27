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

        [SwaggerOperation(Summary = "Add a new friend :)")]
        [HttpPost(), Authorize]
        public IActionResult CreateFriend([FromBody] CreateFriend request)
        {
            int FriendID = request.FriendID;

            // Check if name is already registered
            Friend? existingFriend = _context.Friends.FirstOrDefault(friend => friend.FriendID == FriendID);
            if (existingFriend != null)
            {
                return BadRequest(Helper.Helper.GenerateError("Friend with this name already exists"));
            }

            // Create friend
            Friend friend = new Friend
            {
                FriendID = FriendID,
                AddedAt = DateTime.UtcNow
            };

            _context.Friends.Add(friend);
            _context.SaveChanges();
            return Ok(friend);
        }

        [SwaggerOperation(Summary = "Delete a specific friend")]
        [HttpDelete("{id}"), Authorize]
        public IActionResult Deletefriend(int id)
        {
            Friend? friend = _context.Friends.Find(id);
            if (friend == null)
            {
                return NotFound(Helper.Helper.GenerateError("friend not found"));
            }
            _context.Friends.Remove(friend);
            _context.SaveChanges();
            return Ok(friend);
        }
    }
}
