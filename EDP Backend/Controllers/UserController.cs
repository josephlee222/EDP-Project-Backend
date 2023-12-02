using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EDP_Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using Microsoft.IdentityModel.Tokens;

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
                return BadRequest("Email already registered");
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
                return BadRequest("Email not registered");
            }

            // Check if password is correct
            bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(request.Password, existingUser.Password);
            if (!isPasswordCorrect)
            {
                return BadRequest("Password incorrect");
            }
            var token = CreateToken(existingUser);
            return Ok(new { user = existingUser, token });
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