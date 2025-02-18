using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using StockMaster.Models;
using System;
using System.Linq;
using System.Security.Claims;

namespace StockMaster.Controllers.Api
{
    [Route("api/user")]
    [ApiController]
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
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (model == null)
            {
                return BadRequest(new { Message = "Invalid request. Missing body." });
            }

            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
            {
                return BadRequest(new { Message = "Email and Password are required." });
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return Unauthorized(new { Message = "Invalid email or password." });
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!isPasswordValid)
            {
                return Unauthorized(new { Message = "Invalid email or password." });
            }

            await _signInManager.SignInAsync(user, isPersistent: true);

            return Ok(new { Message = "Login successful.", UserDetails = new { user.Email, user.UserName } });
        }


        // ✅ LOGOUT USER
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { Message = "Logout successful." });
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
