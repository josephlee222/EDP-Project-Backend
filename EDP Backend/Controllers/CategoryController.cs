using EDP_Backend.Models;
using EDP_Backend.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;


namespace EDP_Backend.Controllers.Admin
{
    [ApiController]
    [Route("/Category")]
    public class CategoryController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly IConfiguration _configuration;

        public CategoryController(MyDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [SwaggerOperation(Summary = "Get all current categories")]
        [HttpGet(), /*Authorize(Roles = "Admin")*/]
        public IActionResult GetCategories()
        {
            return Ok(_context.Categories);
        }


        [SwaggerOperation(Summary = "Get a specific category")]
        [HttpGet("{id}")]
        public IActionResult Getcategory(int id)
        {
            Category? category = _context.Categories.Find(id);
            if (category == null)
            {
                return NotFound(Helper.Helper.GenerateError("category not found"));
            }
            return Ok(category);
        }
    }
}
