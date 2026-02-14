using Dapper;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PaymentApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly IDbConnection _connection;
        private readonly AuthOptions _authOptions;
        public AuthService(IDbConnection connection, AuthOptions authOptions)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _authOptions = authOptions ?? throw new ArgumentNullException(nameof(authOptions));
        }
        public async Task<string> Login(string cardToken)
        {
            string sql = "SELECT 1 FROM public.cards WHERE card_token = @cardToken";

            if(await _connection.ExecuteScalarAsync<int?>(sql, new { cardToken }) is null) 
                throw new Exception("Доступ запрещен");

            var claims = new List<Claim> { new("card_token", cardToken) };

            var jwt = new JwtSecurityToken(
                issuer: _authOptions.Issuer,
                audience: _authOptions.Audience,
                claims: claims,
                expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(2)),
                signingCredentials: new SigningCredentials(_authOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return encodedJwt;
        }
    }
}
