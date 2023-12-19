using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EDP_Backend.Models;
using EDP_Backend.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Annotations;

namespace EDP_Backend.Controllers
{
    [ApiController]
    [Route("Admin/User")]
    public class UserAdminController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly IConfiguration _configuration;
        public UserAdminController(MyDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [SwaggerOperation(Summary = "Create a new user")]
        [HttpPost("Create"), Authorize(Roles = "Admin")]
        public IActionResult CreateUser([FromBody] CreateRequest request)
        {
            string? name = request.Name.Trim();
            string? email = request.Email.Trim();

            // Check if email is already registered
            User? existingUser = _context.Users.FirstOrDefault(user => user.Email == email);
            if (existingUser != null)
            {
                return BadRequest(Helper.Helper.GenerateError("Account with this email already exists"));
            }

            // Create user
            User user = new User
            {
                Name = name,
                Email = email
            };

            _context.Users.Add(user);
            _context.SaveChanges();
            return Ok(user);
        }

        [SwaggerOperation(Summary = "Get all current users")]
        [HttpGet(), Authorize(Roles = "Admin")]
        public IActionResult GetUsers()
        {
            return Ok(_context.Users);
        }

        [SwaggerOperation(Summary = "Get a specific user")]
        [HttpGet("{id}"), Authorize(Roles = "Admin")]
        public IActionResult GetUser(int id)
        {
            User? user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound(Helper.Helper.GenerateError("User not found"));
            }
            return Ok(user);
        }
    }
}