using CafeWeb.Models;
using Dapper;
using Npgsql;
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
                (int id, string hash, bool isAdmin)= await _connection.QuerySingleAsync<(int, string, bool)>(
                    @"SELECT 
                    user_id AS id,
                    password,
                    is_admin AS isAdmin
                    FROM public.users
                    WHERE login = @Login", new { user.Login });

                if (!_passwordService.VerifyPassword(user.Password.Trim(), hash.Trim()))
                    throw new UnauthorizedAccessException("Неправильный логин или пароль");

                string role = isAdmin ? "admin" : "user";
                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, id.ToString()),
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
                _logger.LogError("Ошибка при входе в аккаунт: {message}", ex.Message);
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
                _logger.LogError("Ошибка при создании аккаунта: {message}", ex.Message);
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
                _logger.LogError("Ошибка при получении данных о логине: {message}", ex.Message);
                throw;
            }
        }

        public async Task<List<OfferUserModel>> GetOffers()
        {
            try
            {
                var offerDictionary = new Dictionary<string, OfferUserModel>();

                await _connection.QueryAsync<Offer, Food, OfferUserModel>(
                    @"SELECT 
                        o.offer_id AS Id,
                        o.offer_name AS Name,
                        o.description AS Description,
                        o.discount AS Discount,
                        o.starts_at::TIMESTAMP AS StartsAt,
                        o.ends_at::TIMESTAMP AS EndsAt,
                        f.food_id AS Id,
                        f.food_name AS Name,
                        f.price AS Price,
                        f.calories AS Calories,
                        f.weight AS Weight,
                        f.ingredients AS Ingredients,
                        f.description AS Description,
                        f.front_image_address AS FrontImageAddress,
                        f.back_image_address AS BackImageAddress,
                        f.category_id AS CategoryId
                    FROM public.offers o
                    LEFT JOIN public.offers_food of ON o.offer_id = of.offer_id
                    LEFT JOIN public.food f ON of.food_id = f.food_id
                    WHERE o.ends_at >= CURRENT_DATE
                    ORDER BY o.offer_id, f.food_id",
                    (offer, food) =>
                    {
                        if (!offerDictionary.TryGetValue(offer.Name, out var currentOffer))
                        {
                            currentOffer = new OfferUserModel
                            {
                                Offer = offer,
                                Foods = []
                            };
                            offerDictionary.Add(currentOffer.Offer.Name, currentOffer);
                        }

                        if (food != null && food.Id > 0)
                            currentOffer.Foods.Add(food);

                        return currentOffer;
                    },
                    splitOn: "Id"
                );

                return offerDictionary.Values.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError("Не удалось получить акции: {message}", ex.Message);
                throw;
            }
        }

        public async Task<bool> ChangePassword(int userId, string current, string newPassword)
        {
            try
            {
                string? hash = await _connection.QueryFirstOrDefaultAsync<string>(@"
                    SELECT password FROM public.users
                    WHERE user_id = @UserId", new { UserId = userId });

                if (string.IsNullOrWhiteSpace(hash))
                    throw new KeyNotFoundException("Не удалось получить данные пользователя");

                if (!_passwordService.VerifyPassword(current, hash))
                    return false;

                int affected = await _connection.ExecuteAsync(@"
                    UPDATE public.users 
                    SET password = @NewPassword
                    WHERE user_id = @UserId",
                    new
                    {
                        NewPassword = _passwordService.HashPassword(newPassword),
                        UserId = userId
                    });

                if (affected <= 0)
                    throw new Exception("Не удалось обновить пароль");

                return true;
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch(Exception ex)
            {
                _logger.LogError("Не удалось обновить пароль: {message}", ex.Message);
                throw;
            }
        }
    }
}
