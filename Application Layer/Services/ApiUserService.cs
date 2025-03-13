using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using StockMaster.Application_Layer.Interfaces;
using StockMaster.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace StockMaster.Application_Layer.Services
{
    public class ApiUserService : IApiUserService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public ApiUserService(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        // ✅ Register User
        public async Task<object> Register(RegisterModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Password))
                return new { Message = "Invalid registration details." };

            var user = new IdentityUser { UserName = model.Email, Email = model.Email };

            try
            {
                var result = await _userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                    return new { Message = "Registration failed.", Errors = result.Errors.Select(e => e.Description) };

                await _userManager.AddToRoleAsync(user, "User");
                return new { Message = $"User {model.Email} registered successfully." };
            }
            catch (Exception ex)
            {
                return new { Message = "An error occurred during registration.", Error = ex.Message };
            }
        }

        // ✅ Login & Generate Token
        public async Task<object> Login(LoginModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                return new { Message = "Invalid email or password" };

            var token = await GenerateJwtToken(user);
            var roles = await _userManager.GetRolesAsync(user);

            return new
            {
                Token = token,
                Email = user.Email,
                UserName = user.UserName,
                Roles = roles
            };
        }

        // ✅ Get User Details
        public async Task<object> GetUserDetails(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return new { Message = "User not found." };

            var roles = await _userManager.GetRolesAsync(user);
            var token = await GenerateJwtToken(user);

            return new
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                Roles = roles,
                Token = token
            };
        }

        // ✅ Get User Count
        public int GetUserCount()
        {
            return _userManager.Users.Count();
        }

        // ✅ Generate JWT Token
        private async Task<string> GenerateJwtToken(IdentityUser user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpiryMinutes"] ?? "60")),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
