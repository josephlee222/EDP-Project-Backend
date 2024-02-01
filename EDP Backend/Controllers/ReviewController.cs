using EDP_Backend.Models;
using EDP_Backend.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;


namespace EDP_Backend.Controllers.Admin
{
    [ApiController]
    [Route("/Review")]
    public class ReviewController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly IConfiguration _configuration;

        public ReviewController(MyDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [SwaggerOperation(Summary = "Get all current reviews")]
        [HttpGet()]
        public IActionResult GetReviews()
        {
            return Ok(_context.Reviews);
        }


        [SwaggerOperation(Summary = "Create a new review")]
        [HttpPost()]
        public IActionResult CreateReview([FromBody] CreateReviewRequest request)
        {
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value);

            int activityId = request.ActivityId;
            int rating = request.Rating;
            string? description = request.Description;

            // Check if name is already registered
            Review? existingReview = _context.Reviews.FirstOrDefault(review => 
            review.UserId == userId && review.ActivityId == activityId && review.CreatedAt == DateTime.Now);
            if (existingReview != null)
            {
                return BadRequest(Helper.Helper.GenerateError("Review with this name already exists"));
            }

            // Create review
            Review review = new Review
            {
                UserId = userId,
                ActivityId = activityId,
                Rating = rating,
                Description = description ?? "",
            };

            _context.Reviews.Add(review);
            _context.SaveChanges();
            return Ok(review);
        }

        [SwaggerOperation(Summary = "Get all reviews for a specific activity")]
        [HttpGet("Activity/{id}")]
        public IActionResult GetReviewForActivity(int id)
        {
            var review = _context.Reviews.Where(x => x.ActivityId == id);
            if (review == null)
            {
                return NotFound(Helper.Helper.GenerateError("review not found"));
            }
            return Ok(review);
        }

        [SwaggerOperation(Summary = "Get a specific review")]
        [HttpGet("{id}")]
        public IActionResult Getreview(int id)
        {
            Review? review = _context.Reviews.Find(id);
            if (review == null)
            {
                return NotFound(Helper.Helper.GenerateError("review not found"));
            }
            return Ok(review);
        }

        [SwaggerOperation(Summary = "Update a specific review")]
        [HttpPut("{id}"), Authorize]
        public IActionResult Editreview(int id, [FromBody] EditReviewRequest request)
        {
            int UserId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value);

            Review? review = _context.Reviews.Where(x => x.UserId == UserId).FirstOrDefault(x => x.Id == id);


            // Check if review exists
            if (review == null)
            {
                return NotFound(Helper.Helper.GenerateError("review not found"));
            }

            // Update review

            int rating = request.Rating;
            string? description = request.Description;

            review.Rating = rating;
            review.Description = description ?? "";


            _context.SaveChanges();
            return Ok(review);
        }

        [SwaggerOperation(Summary = "Delete a specific review")]
        [HttpDelete("{id}")]
        public IActionResult Deletereview(int id)
        {
            Review? review = _context.Reviews.Find(id);
            if (review == null)
            {
                return NotFound(Helper.Helper.GenerateError("review not found"));
            }
            _context.Reviews.Remove(review);
            _context.SaveChanges();
            return Ok(review);
        }


    }
}
