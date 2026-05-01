using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TravelAPI.Interfaces;
using TravelAPI.Models;

namespace TravelAPI.Controllers
{
    /// <summary>
    /// Controller responsible for handling user authentication and JWT generation.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/> class.
        /// </summary>
        /// <param name="userService">Injected user service for validating credentials.</param>
        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Authenticates a user and returns a JSON Web Token (JWT).
        /// </summary>
        /// <param name="login">The user credentials (Username and Password).</param>
        /// <returns>A JWT token if successful; otherwise, 401 Unauthorized.</returns>
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel login)
        {
            // 1. Check if the user exists in our users.json file
            var user = _userService.Authenticate(login.Username, login.Password);

            // 2. If user is null, authentication failed
            if (user == null)
            {
                return Unauthorized("Invalid username or password.");
            }

            // 3. Authentication successful - Generate the JWT Token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("SuperSecretKeyThatIsAtLeast32BytesLong!");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                // Add claims (payload data) to the token
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role) // Adding the role is a great practice!
                }),

                // Token expiration set to 1 hour
                Expires = DateTime.UtcNow.AddHours(1),

                // Sign the token
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            // Return the generated token to the client
            return Ok(new { token = tokenHandler.WriteToken(token) });
        }
    }
}