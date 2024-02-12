using EDP_Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace EDP_Backend.Controllers
{
    [ApiController]
    [Route("/FriendRequest")]
    public class FriendRequestController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly IConfiguration _configuration;

        public FriendRequestController(MyDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

		//TODO: Add filter for id
		[SwaggerOperation(Summary = "Get all current friend requests")]
        [HttpGet("{RecipientID}"), Authorize]
        public IActionResult GetFriendRequests(int RecipientID)
        {
            return Ok(_context.FriendRequests);
        }

		//TODO: Complete Get by searching for both sender and recipient iD
		[SwaggerOperation(Summary = "Get a specific friend request")]
        [HttpGet("{recipientID}"), Authorize]
        public IActionResult GetFriendRequest(int recipientID)
        {
            FriendRequest? FriendRequest = _context.FriendRequests.Find(recipientID);
            if (FriendRequest == null)
            {
                return NotFound(Helper.Helper.GenerateError("Friend Request not found"));
            }
            return Ok(FriendRequest);
        }

        [SwaggerOperation(Summary = "Add a new friend request :)")]
        [HttpPost(), Authorize]
        public IActionResult CreateFriendRequest([FromBody] FriendRequest request)
        {
            int SenderID = request.SenderID;
            int RecipientID = request.RecipientID;

            // Check if friend/friend request already exists
            Friend? existingFriend = _context.Friends.FirstOrDefault(friend => friend.SenderID == SenderID && friend.RecipientID == RecipientID);
            FriendRequest? existingFriendRequest = _context.FriendRequests.FirstOrDefault(friendrequest => friendrequest.SenderID == SenderID && friendrequest.RecipientID == RecipientID);
            if (existingFriend != null && existingFriendRequest != null)
            {
                return BadRequest(Helper.Helper.GenerateError("You are already friends"));
            }

            // Create friendrequest
            FriendRequest friendrequest = new FriendRequest
            {
                SenderID = SenderID,
                RecipientID = RecipientID,
                SentAt = DateTime.UtcNow
            };

            _context.FriendRequests.Add(friendrequest);
            _context.SaveChanges();
            return Ok(friendrequest);
        }

        [SwaggerOperation(Summary = "Delete a specific friend request")]
        [HttpDelete("{id}"), Authorize]
        public IActionResult Deletefriendrequest(int id)
        {
            FriendRequest? friendrequest = _context.FriendRequests.Find(id);
            if (friendrequest == null)
            {
                return NotFound(Helper.Helper.GenerateError("Friend Request not found"));
            }
            _context.FriendRequests.Remove(friendrequest);
            _context.SaveChanges();
            return Ok(friendrequest);
        }
    }
}