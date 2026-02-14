using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace PaymentApi
{
    public class AuthOptions
    {
        public string Issuer { get; }
        public string Audience { get; }
        public string Key { get; }

        public AuthOptions(IConfiguration configuration)
        {
            var authSection = configuration.GetSection("Auth");
            Issuer = authSection["Issuer"] ?? "payment-api";
            Audience = authSection["Audience"] ?? "cafe";
            Key = authSection["Key"] ?? throw new Exception("Auth:Key not configured");
        }
        public SymmetricSecurityKey GetSymmetricSecurityKey() =>
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));
    }
}
