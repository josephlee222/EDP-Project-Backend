using EDP_Backend.Models;
using EDP_Backend.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;


namespace EDP_Backend.Controllers.Admin
{
    [ApiController]
    [Route("Admin/Category")]
    public class CategoryAdminController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly IConfiguration _configuration;

        public CategoryAdminController(MyDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [SwaggerOperation(Summary = "Get all current categories")]
        [HttpGet(), Authorize(Roles = "Admin")]
        public IActionResult GetCategories()
        {
            return Ok(_context.Categories);
        }


        [SwaggerOperation(Summary = "Create a new category")]
        [HttpPost(), Authorize(Roles = "Admin")]
        public IActionResult CreateCategory([FromBody] CreateCategoryRequest request)
        {
            string name = request.Name;
            string? description = request.Description;

            // Check if name is already registered
            Category? existingCategory = _context.Categories.FirstOrDefault(category => 
            category.Name == name);
            if (existingCategory != null)
            {
                return BadRequest(Helper.Helper.GenerateError("Category with this name already exists"));
            }

            // Create category
            Category category = new Category
            {
                Name = name,
                Description = description ?? "",
            };

            _context.Categories.Add(category);
            _context.SaveChanges();
            return Ok(category);
        }


        [SwaggerOperation(Summary = "Get a specific category")]
        [HttpGet("{id}"), Authorize(Roles = "Admin")]
        public IActionResult Getcategory(int id)
        {
            Category? category = _context.Categories.Find(id);
            if (category == null)
            {
                return NotFound(Helper.Helper.GenerateError("category not found"));
            }
            return Ok(category);
        }

        [SwaggerOperation(Summary = "Update a specific category")]
        [HttpPut("{id}"), Authorize(Roles = "Admin")]
        public IActionResult Editcategory(int id, [FromBody] EditCategoryRequest request)
        {
            // Get category
            Category? category = _context.Categories.Find(id);

            // Check if category exists
            if (category == null)
            {
                return NotFound(Helper.Helper.GenerateError("category not found"));
            }

            // Update category

            string name = request.Name;
            string? description = request.Description;

            category.Name = name;
            category.Description = description ?? "";


            _context.SaveChanges();
            return Ok(category);
        }

        [SwaggerOperation(Summary = "Delete a specific category")]
        [HttpDelete("{id}"), Authorize(Roles = "Admin")]
        public IActionResult Deletecategory(int id)
        {
            Category? category = _context.Categories.Find(id);
            if (category == null)
            {
                return NotFound(Helper.Helper.GenerateError("category not found"));
            }
            _context.Categories.Remove(category);
            _context.SaveChanges();
            return Ok(category);
        }


    }
}
