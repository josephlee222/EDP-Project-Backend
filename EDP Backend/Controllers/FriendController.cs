using EDP_Backend.Models;
using EDP_Backend.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Linq;



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
        [HttpGet("{SenderID}"), Authorize]
        public IActionResult GetFriends(int SenderID)
        {
            return Ok(_context.Friends.Where(friend => friend.SenderID == SenderID || friend.RecipientID == SenderID).ToList());
        }
        
        [SwaggerOperation(Summary = "Get a specific friend")]
        [HttpGet("{SenderID},{RecipientID}"), Authorize]
        public IActionResult GetFriend(int SenderID, int RecipientID)
        {
			Friend? Friend = _context.Friends.FirstOrDefault(friend => (friend.SenderID == SenderID && friend.RecipientID == RecipientID) || (friend.SenderID == RecipientID && friend.RecipientID == SenderID));
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
            //TODO: Find a way to enter in user id for sender id
			int SenderID = request.SenderID;
			int RecipientID = request.RecipientID;
            // Check if friend is already created
            User? existingUser = _context.Users.FirstOrDefault(user => user.Id == RecipientID);
			Friend? existingFriend = _context.Friends.FirstOrDefault(friend => (friend.SenderID == SenderID && friend.RecipientID == RecipientID) || (friend.SenderID == RecipientID && friend.RecipientID == SenderID));
			if (existingUser == null)
			{
				return BadRequest(Helper.Helper.GenerateError("User does not exist"));
			}
			else if (existingFriend != null)
            {
                return BadRequest(Helper.Helper.GenerateError("Friend with this name already exists"));
            }
            // Create friend
            Friend friend = new Friend
            {
                SenderID = SenderID,
				RecipientID = RecipientID,
                Name = existingUser.Name,
				ProfilePicture = existingUser.ProfilePicture,
				AddedAt = DateTime.UtcNow
            };

            _context.Friends.Add(friend);
            _context.SaveChanges();
            return Ok(friend);
        }

        [SwaggerOperation(Summary = "Delete a specific friend")]
        [HttpDelete("{SenderID},{RecipientID}"), Authorize]
        public IActionResult Deletefriend(int SenderID, int RecipientID)
		{
			Friend? friend = _context.Friends.FirstOrDefault(friend => (friend.SenderID == SenderID && friend.RecipientID == RecipientID) || (friend.SenderID == RecipientID && friend.RecipientID == SenderID));
			if (friend == null)
            {
                return NotFound(Helper.Helper.GenerateError("Friend not found"));
            }
            _context.Friends.Remove(friend);
            _context.SaveChanges();
            return Ok(friend);
        }
    }
}
