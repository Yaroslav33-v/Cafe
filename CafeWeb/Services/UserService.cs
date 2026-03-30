using CafeWeb.Models;
using Dapper;
using System.Data;
using System.Security.Claims;

namespace CafeWeb.Services
{
    public class UserService : IUserService
    {
        // Логин: admin Пароль: admin
        private readonly IDbConnection _connection;
        private readonly ILogger<UserService> _logger;
        private readonly IPasswordService _passwordService;
        public UserService(IDbConnection connection, ILogger<UserService> logger, IPasswordService passwordService)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
        }
        public async Task<ClaimsIdentity> SignIn(User user)
        {
            try
            {
                (string hash, bool isAdmin)= await _connection.QuerySingleAsync<(string, bool)>(
                    @"SELECT 
                    password,
                    is_admin AS isAdmin
                    FROM public.users
                    WHERE login = @Login", new { user.Login });

                if (!_passwordService.VerifyPassword(user.Password.Trim(), hash.Trim()))
                    throw new UnauthorizedAccessException("Неправильный логин или пароль");

                string role = isAdmin ? "admin" : "user"; 
                var claims = new List<Claim>
                {
                    new(ClaimTypes.Name, user.Login),
                    new(ClaimTypes.Role, role)
                };

                return new ClaimsIdentity(claims, "Cookies");
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch(InvalidOperationException)
            {
                throw new UnauthorizedAccessException("Aккаунта с такими данными не существует!");
            }
            catch (Exception ex)
            {
                _logger.LogError("Ошибка при входе в аккаунт: " + ex.Message);
                throw new Exception("Проблема при входе в аккаунт");
            }
        }

        public async Task SignUp(User user)
        {
            try
            {
                string hashedPassword = _passwordService.HashPassword(user.Password);

                await _connection.ExecuteAsync(
                    @"INSERT INTO public.users (login, password)
                    VALUES (@Login, @Password)",
                    new { user.Login, Password = hashedPassword });
            }
            catch (Exception ex)
            {
                _logger.LogError("Ошибка при создании аккаунта: " + ex.Message);
                throw new Exception("Ошибка при создании аккаунта. Попробуйте позже");
            }
        }

        public async Task<bool> IsNewLogin(string login)
        {
            try
            {
                bool loginFinded = await _connection.QuerySingleOrDefaultAsync<bool>(
                    @"SELECT 1 FROM public.users 
                    WHERE login = @Login", 
                    new { login });

                return !loginFinded;
            }
            catch(Exception ex) 
            {
                _logger.LogError("Ошибка при получении данных о логине: " + ex.Message);
                throw;
            }
        }
    }
}
