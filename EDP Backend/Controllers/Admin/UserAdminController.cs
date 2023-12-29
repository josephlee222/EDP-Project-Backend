using EDP_Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        [HttpPost(), Authorize(Roles = "Admin")]
        public IActionResult CreateUser([FromBody] CreateUserRequest request)
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
                Email = email,
                IsAdmin = request.IsAdmin
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            user = _context.Users.FirstOrDefault(user => user.Email == request.Email);
            Token token = new()
            {
                User = user,
                Type = "Verify"
            };

            var website = Environment.GetEnvironmentVariable("NET_WEBSITE");

            Helper.Helper.SendMail(user.Name, user.Email, "Activate your UPlay Account", @$"<h1>Verify and activate your UPlay account</h1><br><p>Thank you for signing up for UPlay. Please click on the link below to activate your account. (This email design is temporary and subject to change)</p><br>{website}/verify?t={token.Code}");

            _context.Tokens.Add(token);
            _context.SaveChanges();
            return Ok(user);
        }

        [SwaggerOperation(Summary = "Get all current users")]
        [HttpGet(), Authorize(Roles = "Admin")]
        public IActionResult GetUsers()
        {
            return Ok(_context.Users.Where(user => !user.IsDeleted));
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

        [SwaggerOperation(Summary = "Update a specific user (Needs fixing)")]
        [HttpPut("{id}"), Authorize(Roles = "Admin")]
        public IActionResult UpdateUser(int id, [FromBody] AdminEditUserRequest request)
        {
            User? user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound(Helper.Helper.GenerateError("User not found"));
            }
            

            // Update user if fields are not null
            user.Name = request.Name ?? user.Name;
            user.Email = request.Email ?? user.Email;
            user.IsAdmin = request.IsAdmin ?? user.IsAdmin;
            user.PhoneNumber = request.PhoneNumber ?? user.PhoneNumber;
            user.Address = request.Address ?? user.Address;
            user.OccupationalStatus = request.OccupationalStatus ?? user.OccupationalStatus;
            user.ProfilePictureType = request.ProfilePictureType ?? user.ProfilePictureType;
            user.PostalCode = request.PostalCode ?? user.PostalCode;
            
            // Encrypt password if not null
            if (request.NewPassword != null)
            {
                string encryptedPassword = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                user.Password = encryptedPassword;
            }

            _context.SaveChanges();
            return Ok(user);
        }

        [SwaggerOperation(Summary = "Delete a specific user")]
        [HttpDelete("{id}"), Authorize(Roles = "Admin")]
        public IActionResult DeleteUser(int id)
        {
            User? user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound(Helper.Helper.GenerateError("User not found"));
            }
            user.IsDeleted = true;
            _context.SaveChanges();
            return Ok();
        }
    }
}