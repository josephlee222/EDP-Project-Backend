using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EDP_Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Annotations;

namespace EDP_Backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly IConfiguration _configuration;
        public UserController(MyDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [SwaggerOperation(Summary = "Register with email and password")]
        [AllowAnonymous]
        [HttpPost("Register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            // trim whitespace
            request.Name = request.Name.Trim();
            request.Email = request.Email.Trim();
            request.Password = request.Password.Trim();

            // Check if email is already registered
            User? existingUser = _context.Users.FirstOrDefault(user => user.Email == request.Email);
            if (existingUser != null)
            {
                return BadRequest(Helper.Helper.GenerateError("Account with this email already exists"));
            }

            // Encrypt password
            string encryptedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            User user = new User
            {
                Name = request.Name,
                Email = request.Email,
                Password = encryptedPassword
            };
            _context.Users.Add(user);
            _context.SaveChanges();
            return Ok(user);
        }

        [SwaggerOperation(Summary = "Login with email and password")]
        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // trim whitespace
            request.Email = request.Email.Trim();
            request.Password = request.Password.Trim();

            // Check if email is already registered
            User? existingUser = _context.Users.FirstOrDefault(user => user.Email == request.Email);
            if (existingUser == null)
            {
                return BadRequest(Helper.Helper.GenerateError("Account does not exist"));
            }

            // Check if password is correct
            bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(request.Password, existingUser.Password);
            if (!isPasswordCorrect)
            {
                return BadRequest(Helper.Helper.GenerateError("Incorrect password"));
            }
            var token = CreateToken(existingUser);
            return Ok(new { user = existingUser, token });
        }

        [SwaggerOperation(Summary = "Get user info")]
        [HttpGet("Auth"), Authorize]
        public IActionResult Auth()
        {
            var id = Convert.ToInt32(User.Claims.Where(
            c => c.Type == ClaimTypes.NameIdentifier)
            .Select(c => c.Value).SingleOrDefault());
            var name = User.Claims.Where(c => c.Type == ClaimTypes.Name)
            .Select(c => c.Value).SingleOrDefault();
            var email = User.Claims.Where(c => c.Type == ClaimTypes.Email)
            .Select(c => c.Value).SingleOrDefault();
            if (id != 0 && name != null && email != null)
            {
                var user = new
                {
                    id,
                    email,
                    name
                };
                return Ok(new { user });
            }
            else
            {
                return Unauthorized(new { id, email, name });
            }
        }


        [SwaggerOperation(Summary = "Initialise default admin user if it does not exist")]
        [HttpGet("Init")]
        public IActionResult Init()
        {
            // Create admin user (only works if no default admin exist)
            User? existingUser = _context.Users.FirstOrDefault(user => user.Email == "admin@admin.com");
            if (existingUser == null)
            {
                string encryptedPassword = BCrypt.Net.BCrypt.HashPassword("admin123");
                User user = new User
                {
                    Name = "admin",
                    Email = "admin@admin.com",
                    Password = encryptedPassword,
                    IsVerified = true,
                    IsAdmin = true,
                };

                _context.Users.Add(user);
                _context.SaveChanges();
                return Ok(user);
            } else
            {
                return BadRequest(Helper.Helper.GenerateError("Admin user already exists"));
            }
        }

        private string CreateToken(User user)
        {
            string secret = _configuration.GetValue<string>(
            "Authentication:Secret");
            int tokenExpiresDays = _configuration.GetValue<int>(
            "Authentication:TokenExpiresDays");
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User")
            }),
                Expires = DateTime.UtcNow.AddDays(tokenExpiresDays),
                SigningCredentials = new SigningCredentials(
            new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            string token = tokenHandler.WriteToken(securityToken);
            return token;
        }
    }
}