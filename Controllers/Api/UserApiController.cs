using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;
using StockMaster.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace StockMaster.Controllers.Api
{
    [Route("api/user")]
    [ApiController]
    public class UserApiController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public UserApiController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        // ✅ REGISTER USER
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (model == null) return BadRequest(new { Message = "Invalid request." });
            if (string.IsNullOrWhiteSpace(model.Password)) return BadRequest(new { Message = "Password cannot be empty." });
            if (model.Password != model.ConfirmPassword) return BadRequest(new { Message = "Passwords do not match." });

            var user = new IdentityUser { UserName = model.Email, Email = model.Email };

            try
            {
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // ✅ Assign 'User' role by default
                    await _userManager.AddToRoleAsync(user, "User");

                    return Ok(new { Message = $"New user with the email: {model.Email} has been successfully registered." });
                }
                return BadRequest(new { Message = "Registration failed.", Errors = result.Errors.Select(e => e.Description) });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred during registration.", Error = ex.Message });
            }
        }


        // ✅ LOGIN USER & RETURN JWT
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (model == null) return BadRequest(new { Message = "Invalid request. Missing body." });

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                return Unauthorized(new { Message = "Invalid email or password." });
            }

            // ✅ Generate JWT Token
            var token = await GenerateJwtToken(user);

            return Ok(new
            {
                Message = "Login successful.",
                Token = token,
                UserDetails = new
                {
                    Email = user.Email,
                    UserName = user.UserName,
                    UserId = user.Id
                }
            });
        }

        // ✅ FETCH USER DETAILS (Protected)
        [HttpGet("details/{email}")]
        [Authorize] // ✅ Require authentication
        public async Task<IActionResult> GetUserDetails(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return NotFound(new { Message = "User not found." });

            // ✅ Fetch user roles
            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new
            {
                Email = user.Email,
                UserName = user.UserName,
                Role = roles.FirstOrDefault() ?? "User" // ✅ Default to 'User' if no role is assigned
            });
        }


        // ✅ GET TOTAL NUMBER OF USERS (Protected)
        [HttpGet("count")]
        [Authorize(Roles = "Admin,SuperAdmin")] // ✅ Restrict to Admins
        public IActionResult GetUserCount()
        {
            var userCount = _userManager.Users.Count();
            return Ok(new { TotalUsers = userCount });
        }

        // ==============================================
        // ✅ PRIVATE METHOD TO GENERATE JWT TOKEN
        // ==============================================
        private async Task<string> GenerateJwtToken(IdentityUser user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var jwtKey = jwtSettings["Key"];
            var jwtIssuer = jwtSettings["Issuer"];
            var jwtAudience = jwtSettings["Audience"];
            var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60"); // ✅ Use config value

            if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
            {
                throw new InvalidOperationException("JWT configuration is missing in appsettings.json");
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // ✅ Add user roles as claims
            claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience, // ✅ Fixed audience
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
