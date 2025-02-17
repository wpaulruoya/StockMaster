using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using StockMaster.Models;
using System;
using System.Linq;

namespace StockMaster.Controllers.Api
{
    [Route("api/user")]
    [ApiController] // Ensures it's treated as an API Controller (no view rendering)
    public class UserApiController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public UserApiController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
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
                    return Ok(new { Message = "User registered successfully." });
                }
                return BadRequest(new { Message = "Registration failed.", Errors = result.Errors.Select(e => e.Description) });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred during registration.", Error = ex.Message });
            }
        }

        // ✅ LOGIN USER
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid) return BadRequest(new { Message = "Invalid input.", Errors = ModelState });

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return Unauthorized(new { Message = "Invalid email or password." });

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
            if (!result.Succeeded) return Unauthorized(new { Message = "Invalid email or password." });

            return Ok(new { Message = "Login successful.", UserDetails = new { user.Email, user.UserName } });
        }

        // ✅ FETCH USER DETAILS (By Email)
        [HttpGet("details/{email}")]
        public async Task<IActionResult> GetUserDetails(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return NotFound(new { Message = "User not found." });

            return Ok(new { Email = user.Email, UserName = user.UserName });
        }

        // ✅ GET TOTAL NUMBER OF USERS
        [HttpGet("count")]
        public IActionResult GetUserCount()
        {
            var userCount = _userManager.Users.Count();
            return Ok(new { TotalUsers = userCount });
        }
    }
}
