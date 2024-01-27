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

        [SwaggerOperation(Summary = "Get all current friend requests")]
        [HttpGet(), Authorize]
        public IActionResult GetFriendRequests()
        {
            return Ok(_context.FriendRequests);
        }


        [SwaggerOperation(Summary = "Get a specific friend request")]
        [HttpGet("{id}"), Authorize]
        public IActionResult GetFriendRequest(int id)
        {
            FriendRequest? FriendRequest = _context.FriendRequests.Find(id);
            if (FriendRequest == null)
            {
                return NotFound(Helper.Helper.GenerateError("FriendRequest not found"));
            }
            return Ok(FriendRequest);
        }

        [SwaggerOperation(Summary = "Add a new friend request :)")]
        [HttpPost(), Authorize]
        public IActionResult CreateFriendRequest([FromBody] CreateFriendRequest request)
        {
            int SenderID = request.SenderID;
            int RecipientID = request.RecipientID;

            // Check if name is already registered
            FriendRequest? existingFriendRequest = _context.FriendRequests.FirstOrDefault(friendrequest => friendrequest.SenderID == SenderID && friendrequest.RecipientID == RecipientID);
            if (existingFriendRequest != null)
            {
                return BadRequest(Helper.Helper.GenerateError("FriendRequest with this name already exists"));
            }

            // Create friendrequest
            FriendRequest friendrequest = new FriendRequest
            {
                SenderID = SenderID,
                RecipientID = RecipientID,
                CreatedAt = DateTime.UtcNow
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
                return NotFound(Helper.Helper.GenerateError("friendrequest not found"));
            }
            _context.FriendRequests.Remove(friendrequest);
            _context.SaveChanges();
            return Ok(friendrequest);
        }
    }
}