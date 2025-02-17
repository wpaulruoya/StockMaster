using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using StockMaster.Models;
using System;

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
                return BadRequest(new { Errors = result.Errors });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred.", Error = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return Unauthorized(new { Message = "Invalid credentials." });

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
            if (!result.Succeeded) return Unauthorized(new { Message = "Invalid credentials." });

            return Ok(new { Message = "Login successful" });
        }
    }
}
