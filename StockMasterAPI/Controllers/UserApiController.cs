using Microsoft.AspNetCore.Mvc;
using StockMaster.Application_Layer.Interfaces;
using StockMaster.Models;
using System.Threading.Tasks;

namespace StockMaster.StockMasterAPI.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserApiController : ControllerBase
    {
        private readonly IApiUserService _userService;

        public UserApiController(IApiUserService userService)
        {
            _userService = userService;
        }

        // ✅ Register User
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var result = await _userService.Register(model);
            return Ok(result);
        }

        // ✅ Login User
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var result = await _userService.Login(model);
            return Ok(result);
        }

        // ✅ Get User Details
        [HttpGet("details/{email}")]
        public async Task<IActionResult> GetUserDetails(string email)
        {
            var result = await _userService.GetUserDetails(email);
            return Ok(result);
        }

        // ✅ Get User Count
        [HttpGet("count")]
        public IActionResult GetUserCount()
        {
            var result = _userService.GetUserCount();
            return Ok(new { TotalUsers = result });
        }
    }
}
