using EDP_Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
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
            return Ok(_context.FriendRequests.Where(friendrequest => friendrequest.RecipientID == RecipientID).ToList());
        }

		//TODO: Complete Get by searching for both sender and recipient iD
		[SwaggerOperation(Summary = "Get a specific friend request")]
        [HttpGet("{SenderID},{RecipientID}"), Authorize]
        public IActionResult GetFriendRequest(int SenderID, int RecipientID)
        {
            FriendRequest? FriendRequest = _context.FriendRequests.FirstOrDefault(FriendRequest => FriendRequest.SenderID == SenderID && FriendRequest.RecipientID == RecipientID);
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
            Friend? existingFriend = _context.Friends.FirstOrDefault(friend => (friend.SenderID == SenderID && friend.RecipientID == RecipientID) || (friend.SenderID == RecipientID && friend.RecipientID == SenderID));
            FriendRequest? existingFriendRequest = _context.FriendRequests.FirstOrDefault(friendrequest => (friendrequest.SenderID == SenderID && friendrequest.RecipientID == RecipientID) || (friendrequest.SenderID == RecipientID && friendrequest.RecipientID == SenderID));
            if (existingFriend != null)
            {
                return BadRequest(Helper.Helper.GenerateError("You are already friends"));
            }
			else if (existingFriendRequest != null)
			{
				return BadRequest(Helper.Helper.GenerateError("You have already sent a friend request or received one. Please check your invites"));
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
        [HttpDelete(), Authorize]
        public IActionResult Deletefriendrequest([FromBody] Friend request)
        {
			int SenderID = request.SenderID;
			int RecipientID = request.RecipientID;
			FriendRequest? friendrequest = _context.FriendRequests.FirstOrDefault(friend => (friend.SenderID == SenderID && friend.RecipientID == RecipientID) || (friend.SenderID == RecipientID && friend.RecipientID == SenderID));
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