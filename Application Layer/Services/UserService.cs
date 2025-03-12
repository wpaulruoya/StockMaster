using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using StockMaster.ApplicationLayer.Interfaces;

namespace StockMaster.ApplicationLayer.Services
{
    public class UserService : IUserService
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserService(SignInManager<IdentityUser> signInManager,
                           UserManager<IdentityUser> userManager,
                           RoleManager<IdentityRole> roleManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IdentityResult> RegisterUser(string userName, string email, string password, string confirmPassword)
        {
            if (password != confirmPassword)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Passwords do not match" });
            }

            var user = new IdentityUser
            {
                UserName = userName,
                Email = email
            };

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                // Log errors for debugging
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                Console.WriteLine($"Registration failed: {errors}");
            }

            return result;
        }



        public async Task<SignInResult> AuthenticateUser(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return SignInResult.Failed;

            return await _signInManager.PasswordSignInAsync(user, password, false, false);
        }

        public async Task LogoutUser()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<bool> DeleteUser(string id, IdentityUser currentUser)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null || currentUser.Id == user.Id) return false;

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("SuperAdmin")) return false;

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }
    }
}
