using CafeWeb.Models;

namespace CafeWeb.Services
{
    public interface IUserService
    {
        Task SignUp(User user);
        Task SignIn(User user);
    }
}
