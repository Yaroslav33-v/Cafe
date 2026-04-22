using CafeWeb.Models;
using System.Security.Claims;

namespace CafeWeb.Services
{
    public interface IUserService
    {
        Task SignUp(User user);
        Task<ClaimsIdentity> SignIn(User user);
        Task<bool> IsNewLogin(string login);
        Task<List<OfferUserModel>> GetOffers();
        Task ChangePassword(int userId, string current, string newPassword)
    }
}
