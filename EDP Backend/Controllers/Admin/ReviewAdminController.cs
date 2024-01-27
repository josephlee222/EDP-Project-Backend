using EDP_Backend.Models;
using EDP_Backend.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;


namespace EDP_Backend.Controllers.Admin
{
    [ApiController]
    [Route("Admin/Review")]
    public class ReviewAdminController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly IConfiguration _configuration;

        public ReviewAdminController(MyDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [SwaggerOperation(Summary = "Get all current reviews")]
        [HttpGet(), /*Authorize(Roles = "Admin")*/]
        public IActionResult GetReviews()
        {
            return Ok(_context.Reviews);
        }


        [SwaggerOperation(Summary = "Create a new review")]
        [HttpPost(), Authorize(Roles = "Admin")]
        public IActionResult CreateReview([FromBody] CreateReviewRequest request)
        {
            int userId = request.UserId;
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


        [SwaggerOperation(Summary = "Get a specific review")]
        [HttpGet("{id}"), Authorize(Roles = "Admin")]
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
        [HttpPut("{id}"), Authorize(Roles = "Admin")]
        public IActionResult Editreview(int id, [FromBody] EditReviewRequest request)
        {
            // Get review
            Review? review = _context.Reviews.Find(id);

            // Check if review exists
            if (review == null)
            {
                return NotFound(Helper.Helper.GenerateError("review not found"));
            }

            // Update review

            int userId = request.UserId;
            int activityId = request.ActivityId;
            int rating = request.Rating;
            string? description = request.Description;

            review.UserId = userId;
            review.ActivityId = activityId;
            review.Rating = rating;
            review.Description = description ?? "";


            _context.SaveChanges();
            return Ok(review);
        }

        [SwaggerOperation(Summary = "Delete a specific review")]
        [HttpDelete("{id}"), Authorize(Roles = "Admin")]
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
