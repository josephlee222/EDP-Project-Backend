using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EDP_Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.EntityFrameworkCore;
using Stripe;
using EDP_Backend.Hubs;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.Http.Json;

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
		private readonly IFido2 _fido2;
		public string from = Environment.GetEnvironmentVariable("NET_MAIL_ADDRESS");
        public string password = Environment.GetEnvironmentVariable("NET_MAIL_PASSWORD");
        public string server = Environment.GetEnvironmentVariable("NET_MAIL_SERVER");
        public int port = Convert.ToInt32(Environment.GetEnvironmentVariable("NET_MAIL_PORT"));
        public UserController(MyDbContext context, IConfiguration configuration, IHubContext<ActionsHub> hubContext, IFido2 fido2)
        {
            _context = context;
            _configuration = configuration;
            _hubContext = hubContext;
            _fido2 = fido2;
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
            User? existingUser = _context.Users.Include(user => user.Notifications).AsNoTracking().FirstOrDefault(user => user.Email == request.Email);
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

            // Fliter out read notifications
            existingUser.Notifications = existingUser.Notifications.OrderByDescending(n => n.CreatedAt).Where(notification => !notification.Read).ToList();

            var token = CreateToken(existingUser);
            return Ok(new { user = existingUser, token });
        }

        [SwaggerOperation(Summary = "Login/Register with Google OAuth")]
        [AllowAnonymous]
        [HttpPost("Google")]
        public IActionResult Google([FromBody] OAuthRequest request)
        {
            // trim whitespace
            request.Token = request.Token.Trim();

            // Request google profile
            var client = new HttpClient();
            var response = client.GetAsync($"https://www.googleapis.com/oauth2/v3/userinfo?access_token={request.Token}").Result;
            var content = response.Content.ReadAsStringAsync().Result;
            var googleProfile = JsonConvert.DeserializeObject<GoogleProfile>(content);
            

            // Check if sub is already registered
            User? existingUser = _context.Users.Include(user => user.Notifications).AsNoTracking().FirstOrDefault(user => user.GoogleId == googleProfile.Sub);
            if (existingUser == null)
            {
                // Check if email is already registered
                existingUser = _context.Users.Include(user => user.Notifications).AsNoTracking().FirstOrDefault(user => user.Email == googleProfile.Email);

                if (existingUser != null)
                {
                    // Deny registration if email is already registered
                    return BadRequest(Helper.Helper.GenerateError("This Google e-mail address is already registered with another account"));
                }
                
                // Register user
                User user = new User
                {
                    Name = googleProfile.Name,
                    Email = googleProfile.Email,
                    GoogleId = googleProfile.Sub,
                    IsVerified = true
                };

                _context.Users.Add(user);
                _context.SaveChanges();

                existingUser = _context.Users.FirstOrDefault(user => user.Email == googleProfile.Email);
            }

            // Check if user is not verified
            if (!existingUser.IsVerified)
            {
                return BadRequest(Helper.Helper.GenerateError("Account is not verified. Please check your e-mail inbox"));
            }

            // Check if the user matches the google profile
            if (existingUser.GoogleId != googleProfile.Sub)
            {
                return BadRequest(Helper.Helper.GenerateError("Unable to login with this Google account"));
            }

            // Fliter out read notifications
            existingUser.Notifications = existingUser.Notifications.OrderByDescending(n => n.CreatedAt).Where(notification => !notification.Read).ToList();

            var token = CreateToken(existingUser);
            return Ok(new { user = existingUser, token });
        }


        [SwaggerOperation(Summary = "Login/Register with Facebook OAuth")]
        [AllowAnonymous]
        [HttpPost("Facebook")]
        public IActionResult Facebook([FromBody] OAuthRequest request)
        {
            // trim whitespace
            request.Token = request.Token.Trim();

            // Request Facebook profile
            var client = new HttpClient();
            var response = client.GetAsync($"https://graph.facebook.com/me?fields=id,name,email&access_token={request.Token}").Result;
            var content = response.Content.ReadAsStringAsync().Result;
            var facebookProfile = JsonConvert.DeserializeObject<FacebookProfile>(content);


            // Check if sub is already registered
            User? existingUser = _context.Users.Include(user => user.Notifications).AsNoTracking().FirstOrDefault(user => user.FacebookId == facebookProfile.Id);
            if (existingUser == null)
            {
                // Check if email is already registered
                existingUser = _context.Users.Include(user => user.Notifications).AsNoTracking().FirstOrDefault(user => user.Email == facebookProfile.Email);

                if (existingUser != null)
                {
                    // Deny registration if email is already registered
                    return BadRequest(Helper.Helper.GenerateError("The Facebook account E-mail address is already registered with another account"));
                }

                // Register user
                User user = new User
                {
                    Name = facebookProfile.Name,
                    Email = facebookProfile.Email,
                    FacebookId = facebookProfile.Id,
                    IsVerified = true
                };

                _context.Users.Add(user);
                _context.SaveChanges();

                existingUser = _context.Users.FirstOrDefault(user => user.Email == facebookProfile.Email);
            }

            // Check if user is not verified
            if (!existingUser.IsVerified)
            {
                return BadRequest(Helper.Helper.GenerateError("Account is not verified. Please check your e-mail inbox"));
            }

            // Check if the user matches the google profile
            if (existingUser.FacebookId != facebookProfile.Id)
            {
                return BadRequest(Helper.Helper.GenerateError("Unable to login with this Google account"));
            }

            // Fliter out read notifications
            existingUser.Notifications = existingUser.Notifications.OrderByDescending(n => n.CreatedAt).Where(notification => !notification.Read).ToList();

            var token = CreateToken(existingUser);
            return Ok(new { user = existingUser, token });
        }

        [SwaggerOperation(Summary = "Link/unlink a Facebook account to user")]
        [HttpPost("Facebook/Link"), Authorize]
        public IActionResult LinkFacebook([FromBody] OAuthRequest request)
        {
            int id = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value);
            User? user = _context.Users.FirstOrDefault(user => user.Id == id);

            if (user == null)
            {
                return BadRequest(Helper.Helper.GenerateError("Invalid token"));
            }

            // trim whitespace
            request.Token = request.Token.Trim();

            // Request Facebook profile
            var client = new HttpClient();
            var response = client.GetAsync($"https://graph.facebook.com/me?fields=id,name,email&access_token={request.Token}").Result;
            var content = response.Content.ReadAsStringAsync().Result;
            var facebookProfile = JsonConvert.DeserializeObject<FacebookProfile>(content);

            // If user already has a Facebook account, unlink it
            if (user.FacebookId != null)
            {
                // Check if the user matches the facebook profile
                if (user.FacebookId != facebookProfile.Id)
                {
                    return BadRequest(Helper.Helper.GenerateError("Logged in with a different Facebook account"));
                }

                user.FacebookId = null;
                _context.SaveChanges();
                _hubContext.Clients.Groups(user.Id.ToString()).SendAsync("refresh");
                return Ok(new { message = "Facebook account unlinked" });
            }

            // Check if sub is already registered
            User? existingUser = _context.Users.FirstOrDefault(user => user.FacebookId == facebookProfile.Id);
            if (existingUser != null)
            {
                // Deny registration if email is already registered
                return BadRequest(Helper.Helper.GenerateError("This Facebook account is already registered with another account"));
            }

            // Link Facebook account
            user.FacebookId = facebookProfile.Id;
            _context.SaveChanges();
            _hubContext.Clients.Groups(user.Id.ToString()).SendAsync("refresh");

            return Ok(new { message = "Facebook account linked" });
        }

        [SwaggerOperation(Summary = "Link/unlink a Google account to user")]
        [HttpPost("Google/Link"), Authorize]
        public IActionResult LinkGoogle([FromBody] OAuthRequest request) {
            int id = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value);
            User? user = _context.Users.FirstOrDefault(user => user.Id == id);
        
            if (user == null)
            {
                return BadRequest(Helper.Helper.GenerateError("Invalid token"));
            }

            // trim whitespace
            request.Token = request.Token.Trim();

            // Request Google profile
            var client = new HttpClient();
            var response = client.GetAsync($"https://www.googleapis.com/oauth2/v3/userinfo?access_token={request.Token}").Result;
            var content = response.Content.ReadAsStringAsync().Result;
            var googleProfile = JsonConvert.DeserializeObject<GoogleProfile>(content);

            // If user already has a Google account, unlink it
            if (user.GoogleId != null)
            {
                // Check if the user matches the google profile
                if (user.GoogleId != googleProfile.Sub)
                {
                    return BadRequest(Helper.Helper.GenerateError("Logged in with a different Google account"));
                }

                user.GoogleId = null;
                _context.SaveChanges();
                _hubContext.Clients.Groups(user.Id.ToString()).SendAsync("refresh");
                return Ok(new { message = "Google account unlinked" });
            }

            // Check if sub is already registered
            User? existingUser = _context.Users.FirstOrDefault(user => user.GoogleId == googleProfile.Sub);
            if (existingUser != null)
            {
                // Deny registration if sub is already registered
                return BadRequest(Helper.Helper.GenerateError("This Google account is already registered with another account"));
            }

            // Link Google account
            user.GoogleId = googleProfile.Sub;
            _context.SaveChanges();
            _hubContext.Clients.Groups(user.Id.ToString()).SendAsync("refresh");

            return Ok(new { message = "Google account linked" });
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

        [SwaggerOperation(Summary = "Resend a verification email to a email address")]
        [AllowAnonymous]
        [HttpPost("Resend")]
        public IActionResult Resend([FromBody] ForgotRequest request)
        {
            // trim whitespace
            request.Email = request.Email.Trim();

            // Check if email is already registered
            User? existingUser = _context.Users.FirstOrDefault(user => user.Email == request.Email);

            if (existingUser == null || existingUser.IsDeleted)
            {
                return BadRequest(Helper.Helper.GenerateError("E-mail is invaild. Please try again."));
            }

            // Check if email is already verified
            if (existingUser.IsVerified)
            {
                return BadRequest(Helper.Helper.GenerateError("Email already verified"));
            }

            // Check if token already exists, if so, delete it
            Token? existingToken = _context.Tokens.FirstOrDefault(token => token.User == existingUser && token.Type == "Verify");
            if (existingToken != null)
            {
                _context.Tokens.Remove(existingToken);
            }

            // Create new token
            Token token = new()
            {
                User = existingUser,
                Type = "Verify"
            };
            _context.Tokens.Add(token);
            _context.SaveChanges();

            // Send email with token
            var website = Environment.GetEnvironmentVariable("NET_WEBSITE");
            if (website != null)
            {
                   Helper.Helper.SendMail(existingUser.Name, existingUser.Email, "Verify your UPlay Account", @$"<h1>Verify your UPlay account</h1><br><p>Thank you for using UPlay. Please click on the link below to verify your email. (This email design is temporary and subject to change)</p><br>{website}/verify?t={token.Code}");
            }

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
            User? user = _context.Users.Include(user => user.Notifications).AsNoTracking().FirstOrDefault(user => user.Id == id);

            if (user == null)
            {
                return BadRequest(Helper.Helper.GenerateError("Invalid token"));
            }

            // fliter to only get notifications that are not read
            user.Notifications = user.Notifications.OrderByDescending(n => n.CreatedAt).Where(notification => !notification.Read).ToList();

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
            User? user = _context.Users.Include(user => user.Notifications).AsNoTracking().FirstOrDefault(user => user.Id == id);

            if (user != null)
            {
                // fliter to only get notifications that are not read
                user.Notifications = user.Notifications.OrderByDescending(n => n.CreatedAt).Where(notification => !notification.Read).ToList();

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
                        _context.SendNotification(user.Id, "Payment Received", "$" + amount / 100 + " has been added to your balance.", "General", "View Wallet", "/profile/wallet");

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

        [SwaggerOperation(Summary = "Get current user transaction records")]
        [HttpGet("Transactions"), Authorize]
        public IActionResult GetTransactions()
        {
            // Get user id from token
            int id = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value);
            User? user = _context.Users.Include(u => u.Transactions).FirstOrDefault(user => user.Id == id);

            if (user != null)
            {
                return Ok(user.Transactions);
            }
            else
            {
                return BadRequest(Helper.Helper.GenerateError("User does not exist"));
            }
        }


        // ENV variables test + email test
        [SwaggerOperation(Summary = "Test Route, do not use")]
        [HttpGet("Test"), Authorize]
        public IActionResult TestingStuff()
        {
            int id = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value);

            // if id is not null, send notification
            if (id != 0)
            {
                _context.SendNotification(id, "Test Notification", "Test Notification Body", "General", "Go to Profile", "/profile");
            }
            //Helper.Helper.SendMail("Joseph Lee", "facebooklee52@gmail.com", "Test Email", @$"<h1>Test Email with function now</h1><br>{from}");
            return Ok(Environment.GetEnvironmentVariable("TEST_MESSAGE"));
        }

        [SwaggerOperation(Summary = "Mark notification as read")]
        [HttpGet("Notification/Read"), Authorize]
        public async Task<IActionResult> MarkNotificationAsRead([FromQuery] int notificationId)
        {
            int id = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value);
            User? user = _context.Users.Include(u => u.Notifications).FirstOrDefault(user => user.Id == id);

            if (user != null)
            {
                Notification? notification = user.Notifications.FirstOrDefault(n => n.Id == notificationId);
                if (notification != null)
                {
                    notification.Read = true;
                    _context.SaveChanges();
                    await _hubContext.Clients.Groups(user.Id.ToString()).SendAsync("refresh");
                    return Ok();
                }
                else
                {
                    return BadRequest(Helper.Helper.GenerateError("Notification does not exist"));
                }
            }
            else
            {
                return BadRequest(Helper.Helper.GenerateError("User does not exist"));
            }
        }

        [SwaggerOperation(Summary = "Mark all notifications as read")]
        [HttpGet("Notification/ReadAll"), Authorize]
        public async Task<IActionResult> MarkAllNotificationsAsRead()
        {
			int id = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value);
			User? user = _context.Users.Include(u => u.Notifications).FirstOrDefault(user => user.Id == id);

			if (user != null)
            {
				foreach (Notification notification in user.Notifications)
                {
					notification.Read = true;
				}
				_context.SaveChanges();
				await _hubContext.Clients.Groups(user.Id.ToString()).SendAsync("refresh");
				return Ok();
			}
			else
            {
				return BadRequest(Helper.Helper.GenerateError("User does not exist"));
			}
		}

        [SwaggerOperation(Summary = "Make FIDO2 credential options for passwordless sign-in")]
        [HttpPost("Passkey/Setup"), Authorize]
        public async Task<IActionResult> MakeCredentialOptions()
        {
			int id = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value);
			User? user = _context.Users.FirstOrDefault(user => user.Id == id);

			if (user != null)
            {
				Fido2User fido2User = new()
                {
					Id = Encoding.UTF8.GetBytes(user.Id.ToString()),
					Name = user.Email,
					DisplayName = user.Name
                };

                var options = _fido2.RequestNewCredential(fido2User, new List<PublicKeyCredentialDescriptor>()).ToJson();
				return Ok(options);
			}
			else
            {
				return BadRequest(Helper.Helper.GenerateError("User does not exist"));
			}
		}

        [SwaggerOperation(Summary = "Save FIDO2 credentials for passwordless sign-in")]
        [HttpPost("Passkey/Save"), Authorize]
        public async Task<IActionResult> SaveCredentialOptions([FromBody] SaveCredentialsOptionsRequest request)
        {
            var optionsJson = request.Options;
			var options = CredentialCreateOptions.FromJson(optionsJson);
			return Ok(new { request.AttestationResponse, options });
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