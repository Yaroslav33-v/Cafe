using static BCrypt.Net.BCrypt;

namespace CafeWeb.Services
{
    public class PasswordService : IPasswordService
    {
        public string HashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password);

        public bool VerifyPassword(string password, string hash) => Verify(password, hash);
    }
}
