using InventorySales.API.Data;
using InventorySales.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace InventorySales.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(
            AppDbContext context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // POST: api/Auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(User user)
        {
            if (string.IsNullOrWhiteSpace(user.Username))
            {
                return BadRequest("Username is required.");
            }

            if (string.IsNullOrWhiteSpace(user.Password))
            {
                return BadRequest("Password is required.");
            }

            user.Username = user.Username.Trim();

            var existingUser = _context.Users
                .FirstOrDefault(u =>
                    u.Username.ToLower() == user.Username.ToLower());

            if (existingUser != null)
            {
                return BadRequest("Username already exists.");
            }

            // New users will always be Cashier
            user.Role = "Cashier";

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "User registered successfully.",
                user.Id,
                user.Username,
                user.Role
            });
        }

        // POST: api/Auth/login
        [HttpPost("login")]
        public IActionResult Login(User loginUser)
        {
            if (string.IsNullOrWhiteSpace(loginUser.Username) ||
                string.IsNullOrWhiteSpace(loginUser.Password))
            {
                return Unauthorized(
                    "Username and password are required.");
            }

            var username = loginUser.Username.Trim();

            var user = _context.Users
                .FirstOrDefault(u =>
                    u.Username.ToLower() == username.ToLower() &&
                    u.Password == loginUser.Password);

            if (user == null)
            {
                return Unauthorized(
                    "Invalid username or password.");
            }

            var claims = new[]
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.Id.ToString()),

                new Claim(
                    ClaimTypes.Name,
                    user.Username),

                new Claim(
                    ClaimTypes.Role,
                    user.Role)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _configuration["Jwt:Key"]!));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: credentials
            );

            var tokenString =
                new JwtSecurityTokenHandler()
                    .WriteToken(token);

            return Ok(new
            {
                Message = "Login successful.",
                Token = tokenString,
                Username = user.Username,
                Role = user.Role
            });
        }
    }
}