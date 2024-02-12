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

		//TODO: Add filter for id
		[SwaggerOperation(Summary = "Get all current friends")]
        [HttpGet("{SenderID}"), Authorize]
        public IActionResult GetFriends(int SenderID)
        {
            return Ok(_context.Friends);
        }

        //TODO: Complete Get by searching for both sender and recipient iD
        [SwaggerOperation(Summary = "Get a specific friend")]
        [HttpGet(), Authorize]
        public IActionResult GetFriend([FromBody] Friend request)
        {
			int SenderID = request.SenderID;
			int RecipientID = request.RecipientID;
			Friend? Friend = _context.Friends.Find(RecipientID);
            if (Friend == null)
            {
                return NotFound(Helper.Helper.GenerateError("Friend not found"));
            }
            return Ok(Friend);
        }

        [SwaggerOperation(Summary = "Add a new friend :)")]
        [HttpPost(), Authorize]
        public IActionResult CreateFriend([FromBody] Friend request)
        {
			int SenderID = request.SenderID;
			int RecipientID = request.RecipientID;

			// Check if friend is already created
			Friend? existingFriend = _context.Friends.FirstOrDefault(friend => friend.RecipientID == RecipientID);
            if (existingFriend != null)
            {
                return BadRequest(Helper.Helper.GenerateError("Friend with this name already exists"));
            }

            // Create friend
            Friend friend = new Friend
            {
                SenderID = SenderID,
				RecipientID = RecipientID,
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
