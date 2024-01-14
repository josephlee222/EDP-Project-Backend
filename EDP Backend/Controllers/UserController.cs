using System.IdentityModel.Tokens.Jwt;
using MailKit.Net.Smtp;
using System.Security.Claims;
using System.Text;
using EDP_Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;

using Microsoft.IdentityModel.Tokens;
using MimeKit;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore;
using Stripe;
using EDP_Backend.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace EDP_Backend.Controllers
{
    using Token = Models.Token;
    using StripeToken = Stripe.Token;

    [ApiController]
    [Route("/User")]
    public class UserController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly IConfiguration _configuration;
        // import the IHubContext
        private readonly IHubContext<ActionsHub> _hubContext;
        public string from = Environment.GetEnvironmentVariable("NET_MAIL_ADDRESS");
        public string password = Environment.GetEnvironmentVariable("NET_MAIL_PASSWORD");
        public string server = Environment.GetEnvironmentVariable("NET_MAIL_SERVER");
        public int port = Convert.ToInt32(Environment.GetEnvironmentVariable("NET_MAIL_PORT"));
        public UserController(MyDbContext context, IConfiguration configuration, IHubContext<ActionsHub> hubContext)
        {
            _context = context;
            _configuration = configuration;
            _hubContext = hubContext;
        }

        [SwaggerOperation(Summary = "Register with name, email and password")]
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

        [SwaggerOperation(Summary = "Verify and activate account with token")]
        [AllowAnonymous]
        [HttpPost("Verify")]
        public IActionResult Verify([FromBody] VerifyRequest request)
        {
            // trim whitespace
            request.Token = request.Token.Trim();

            // Check if token exists
            Token? existingToken = _context.Tokens.Include(token => token.User).FirstOrDefault(token => token.Code == request.Token);
            if (existingToken == null)
            {
                return BadRequest(Helper.Helper.GenerateError("Invalid token"));
            }

            // Check if token is not expired
            if (existingToken.Expiry < DateTime.Now)
            {
                return BadRequest(Helper.Helper.GenerateError("Token expired"));
            }

            // Check if token is not used
            if (existingToken.IsUsed)
            {
                return BadRequest(Helper.Helper.GenerateError("Token already used"));
            }

            // Check if token is of type Verify
            if (existingToken.Type != "Verify")
            {
                return BadRequest(Helper.Helper.GenerateError("Invalid token"));
            }

            // Verify user
            User? existingUser = _context.Users.FirstOrDefault(user => user.Id == existingToken.User.Id);
            if (existingUser == null)
            {
                return BadRequest(Helper.Helper.GenerateError("Invalid token"));
            }

            if (existingUser.IsVerified)
            {
                return BadRequest(Helper.Helper.GenerateError("Account already verified"));
            }

            if (existingUser.IsDeleted)
            {
                return BadRequest(Helper.Helper.GenerateError("Account deleted"));
            }

            if (existingUser.Password == null)
            {
                if (request.Password == null || request.Password == "")
                {
                    return Conflict(Helper.Helper.GenerateError("Need to set a password."));
                }
                
                // Encrypt password
                string encryptedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
                existingUser.Password = encryptedPassword;
            }

            existingUser.IsVerified = true;
            _context.Users.Update(existingUser);

            // Mark token as used
            existingToken.IsUsed = true;
            _context.Tokens.Update(existingToken);

            _context.SaveChanges();
            return Ok(existingUser);
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
            if (existingUser == null || existingUser.IsDeleted)
            {
                return BadRequest(Helper.Helper.GenerateError("Wrong login details provided. Please try again."));
            }

            // Check if password is correct
            bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(request.Password, existingUser.Password);
            if (!isPasswordCorrect)
            {
                return BadRequest(Helper.Helper.GenerateError("Wrong login details provided. Please try again."));
            }

            // Check if user is not verified
            if (!existingUser.IsVerified || existingUser.Password == null)
            {
                return BadRequest(Helper.Helper.GenerateError("Account is not verified. Please check your e-mail inbox"));
            }

            var token = CreateToken(existingUser);
            return Ok(new { user = existingUser, token });
        }

        [SwaggerOperation(Summary = "Request for a password reset")]
        [AllowAnonymous]
        [HttpPost("Forgot")]
        public IActionResult Forgot([FromBody] ForgotRequest request)
        {
            // trim whitespace
            request.Email = request.Email.Trim();

            // Check if email is already registered
            User? existingUser = _context.Users.FirstOrDefault(user => user.Email == request.Email);
            if (existingUser == null || existingUser.IsDeleted)
            {
                return BadRequest(Helper.Helper.GenerateError("Wrong login details provided. Please try again."));
            }

            Token token = new()
            {
                User = existingUser,
                Type = "Reset"
            };

            var website = Environment.GetEnvironmentVariable("NET_WEBSITE");

            Helper.Helper.SendMail(existingUser.Name, existingUser.Email, "Reset your UPlay Account", @$"<h1>Reset your UPlay account</h1><br><p>Thank you for using UPlay. Please click on the link below to reset your password. (This email design is temporary and subject to change)</p><br>{website}/reset?t={token.Code}");

            _context.Tokens.Add(token);
            _context.SaveChanges();
            return Ok();
        }

        [SwaggerOperation(Summary = "Check whether a token is still valid for use")]
        [AllowAnonymous]
        [HttpGet("Check/{token}")]
        public IActionResult Check(string token)
        {
            // trim whitespace
            token = token.Trim();

            // Check if token exists
            Token? existingToken = _context.Tokens.FirstOrDefault(tokenFound => tokenFound.Code == token);
            if (existingToken == null)
            {
                return BadRequest(Helper.Helper.GenerateError("Invalid token"));
            }

            // Check if token is not expired
            if (existingToken.Expiry < DateTime.Now)
            {
                return BadRequest(Helper.Helper.GenerateError("Token expired"));
            }

            // Check if token is not used
            if (existingToken.IsUsed)
            {
                return BadRequest(Helper.Helper.GenerateError("Token already used"));
            }

            return Ok(existingToken);
        }


        [SwaggerOperation(Summary = "Reset password with token")]
        [AllowAnonymous]
        [HttpPost("Reset")]
        public IActionResult Reset([FromBody] ResetRequest request)
        {
            // trim whitespace
            request.Token = request.Token.Trim();
            request.Password = request.Password.Trim();

            // Check if token exists
            Token? existingToken = _context.Tokens.Include(token => token.User).FirstOrDefault(token => token.Code == request.Token);
            if (existingToken == null)
            {
                return BadRequest(Helper.Helper.GenerateError("Invalid token"));
            }

            // Check if token is not expired
            if (existingToken.Expiry < DateTime.Now)
            {
                return BadRequest(Helper.Helper.GenerateError("Token expired"));
            }

            // Check if token is not used
            if (existingToken.IsUsed)
            {
                return BadRequest(Helper.Helper.GenerateError("Token already used"));
            }

            // Check if token is of type Reset
            if (existingToken.Type != "Reset")
            {
                return BadRequest(Helper.Helper.GenerateError("Invalid token"));
            }

            // Verify user
            User? existingUser = _context.Users.FirstOrDefault(user => user.Id == existingToken.User.Id);
            if (existingUser == null)
            {
                return BadRequest(Helper.Helper.GenerateError("Invalid token"));
            }

            // Hash password
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Update user
            existingUser.Password = hashedPassword;
            _context.Users.Update(existingUser);

            // Mark token as used
            existingToken.IsUsed = true;
            _context.Tokens.Update(existingToken);

            _context.SaveChanges();
            return Ok(existingUser);
        }


        [SwaggerOperation(Summary = "Refresh user token with provided token")]
        [HttpGet("Refresh"), Authorize]
        public IActionResult Refresh()
        {
            int id = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value);
            User? user = _context.Users.FirstOrDefault(user => user.Id == id);

            if (user == null)
            {
                return BadRequest(Helper.Helper.GenerateError("Invalid token"));
            }

            var token = CreateToken(user);
            return Ok(new { user, token });
        }


        [SwaggerOperation(Summary = "Get user info inside of the token")]
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
            }
            else
            {
                return BadRequest(Helper.Helper.GenerateError("Admin user already exists"));
            }
        }


        [SwaggerOperation(Summary = "Get user information based on the token of the logged in user")]
        [HttpGet(), Authorize]
        public IActionResult GetUserInfo()
        {
            int id = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value);
            User? user = _context.Users.FirstOrDefault(user => user.Id == id);

            if (user != null)
            {
                return Ok(user);
            }
            else
            {
                return BadRequest(Helper.Helper.GenerateError("User does not exist"));
            }
        }

        [SwaggerOperation(Summary = "Update user information based on the token of the logged in user")]
        [HttpPut(), Authorize]
        public IActionResult UpdateUserInfo([FromBody] EditUserRequest request)
        {
            int id = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value);
            User? user = _context.Users.FirstOrDefault(user => user.Id == id);

            if (user != null)
            {
                // Update if request is not null
                user.Name = request.Name ?? user.Name;
                user.Address = request.Address ?? user.Address;
                user.PhoneNumber = request.PhoneNumber ?? user.PhoneNumber;
                user.PostalCode = request.PostalCode ?? user.PostalCode;
                user.OccupationalStatus = request.OccupationalStatus ?? user.OccupationalStatus;
                user.ProfilePictureType = request.ProfilePictureType ?? user.ProfilePictureType;
                user.ProfilePicture = request.ProfilePicture ?? user.ProfilePicture;
                user.Newsletter = request.Newsletter ?? user.Newsletter;


                // Check if password is correct if request is not null
                if (request.NewPassword != null)
                {
                    bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(request.Password, user.Password);
                    if (!isPasswordCorrect)
                    {
                        return BadRequest(Helper.Helper.GenerateError("Incorrect password"));
                    }
                    else
                    {
                        // Encrypt password
                        string encryptedPassword = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                        user.Password = encryptedPassword;
                    }
                }

                _context.SaveChanges();
                return Ok(user);
            }
            else
            {
                return BadRequest(Helper.Helper.GenerateError("User does not exist"));
            }

        }

        [SwaggerOperation(Summary = "Initialize a top-up with Stripe")]
        [HttpGet("Wallet/Topup"), Authorize]
        public IActionResult TopupWallet([FromQuery] int amount)
        {
            // Get user id from token
            int id = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value);
            User? user = _context.Users.FirstOrDefault(user => user.Id == id);

            if (user != null)
            {
                var options = new PaymentIntentCreateOptions
                {
                    Amount = amount * 100,
                    Currency = "sgd",
                    Description = "Topup for wallet",
                    Metadata = new Dictionary<string, string>
                    {
                        { "UserId", user.Id.ToString() },
                        { "Email", user.Email },
                        { "Name", user.Name },
                    },
                };

                var service = new PaymentIntentService();
                PaymentIntent paymentIntent = service.Create(options);

                return Ok(new { paymentIntent.ClientSecret, paymentIntent.Amount });
            }
            else
            {
                return BadRequest(Helper.Helper.GenerateError("User does not exist"));
            }
        }


        [SwaggerOperation(Summary = "Webhook for Stripe API payment response on success")]
        [HttpPost("Wallet/Topup/Webhook")]
        public async Task<IActionResult> TopupWalletWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            try
            {
                // Verify webhook signature
                var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], Environment.GetEnvironmentVariable("NET_STRIPE_WEBHOOK_SECRET"));

                if (stripeEvent.Type == Events.PaymentIntentSucceeded)
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    var userId = paymentIntent.Metadata["UserId"];
                    var amount = paymentIntent.Amount;
                    var email = paymentIntent.Metadata["Email"];
                    var name = paymentIntent.Metadata["Name"];

                    // Update user wallet
                    User? user = _context.Users.FirstOrDefault(user => user.Id == Convert.ToInt32(userId));
                    if (user != null)
                    {
                        user.Balance += amount / 100;
                        _context.SaveChanges();

                        // Add transaction record
                        Transaction transaction = new()
                        {
                            User = user,
                            Amount = amount / 100,
                            Type = "Topup",
                            Settled = true,
                        };

                        _context.Transactions.Add(transaction);
                        _context.SaveChanges();

                        // Send signalR notification
                        await _hubContext.Clients.Groups(user.Id.ToString()).SendAsync("refresh");

                        // Send email
                        Helper.Helper.SendMail(name, email, "Topup Successful", @$"<h1>Topup Successful</h1><br><p>Amount: ${amount / 100}</p><br><p>Wallet Balance: ${user.Balance}</p>");
                        
                        return Ok();
                    }
                    else
                    {
                        return BadRequest(Helper.Helper.GenerateError("User does not exist"));
                    }
                }
                else
                {
                    return BadRequest(Helper.Helper.GenerateError("Payment failed"));
                }
            } catch (Exception e)
            {
                return BadRequest(Helper.Helper.GenerateError(e.Message));
            }
        }


        // ENV variables test + email test
        [SwaggerOperation(Summary = "Test Route, do not use")]
        [HttpGet("Test")]
        public IActionResult TestingStuff()
        {
            Helper.Helper.SendMail("Joseph Lee", "facebooklee52@gmail.com", "Test Email", @$"<h1>Test Email with function now</h1><br>{from}");
            return Ok(Environment.GetEnvironmentVariable("TEST_MESSAGE"));
        }

        private string CreateToken(User user)
        {
            string secret = _configuration.GetValue<string>("Authentication:Secret");
            int tokenExpiresDays = _configuration.GetValue<int>("Authentication:TokenExpiresDays");
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