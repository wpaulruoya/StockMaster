using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace StockMaster.ApplicationLayer.Interfaces
{
    public interface IUserService
    {
        Task<IdentityResult> RegisterUser(string userName, string email, string password, string confirmPassword);
        Task<SignInResult> AuthenticateUser(string email, string password);
        Task LogoutUser();
        Task<bool> DeleteUser(string id, IdentityUser currentUser);
    }
}
