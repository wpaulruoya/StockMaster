using System.Threading.Tasks;
using StockMaster.Models;

namespace StockMaster.Application_Layer.Interfaces
{
    public interface IApiUserService
    {
        Task<object> Register(RegisterModel model);
        Task<object> Login(LoginModel model);
        Task<object> GetUserDetails(string email);
        int GetUserCount();
    }
}
