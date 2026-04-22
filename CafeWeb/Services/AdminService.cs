using CafeWeb.Models;
using Dapper;
using Npgsql;
using System.Data;

namespace CafeWeb.Services
{
    public class AdminService : IAdminService
    {
        private readonly IDbConnection _connection; 
        private readonly ILogger<AdminService> _logger;
        private readonly IPasswordService _passwordService;

        public AdminService(IDbConnection connection, ILogger<AdminService> logger, IPasswordService passwordService)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
        }
         
        public async Task<Dictionary<int, FoodShortModel>> GetAllFood()
        {
            var results = await _connection.QueryAsync(@"
                SELECT food_id, food_name AS Name, front_image_address AS FrontImageAddress, back_image_address AS BackImageAddress
                FROM public.food");

            return results.ToDictionary(
                key => (int)key.food_id,
                value => new FoodShortModel
                {
                    Name = value.name,
                    FrontImageAddress = value.frontimageaddress,
                    BackImageAddress = value.backimageaddress
                }
            );
        }
        
        public async Task<List<string>> GetCategoryNames()
        {
            IEnumerable<string> categoryNames = await _connection.QueryAsync<string>("SELECT name FROM public.categories");

            return categoryNames.ToList();
        }

        public async Task InsertAdmin(User user)
        {
            _logger.LogInformation("Получена заявка на регистрацию нового админа: {Login}", user.Login);
            try
            {
                string hashedPassword = _passwordService.HashPassword(user.Password);
                await _connection.ExecuteAsync(@"INSERT INTO public.users (login, password, is_admin) VALUES
                    (@Login, @Password, @IsAdmin)",
                    new
                    {
                        user.Login,
                        Password = hashedPassword,
                        IsAdmin = true
                    });

                _logger.LogInformation("Новый админ {Login} добавлен в БД", user.Login);
            }
            catch (PostgresException ex) when (ex.Message.Contains("значение ключа нарушает ограничение уникальности"))
            {
                _logger.LogInformation("Логин '{Login}' уже существует", user.Login);
                throw new ArgumentException("Такой логин уже существует");
            }
            catch (PostgresException ex)
            {
                _logger.LogError("Внутренняя ошибка в базе данных: {message}", ex.Message);
                throw new Exception("Внутренняя ошибка в базе данных");
            }
            catch (Exception ex)
            {
                _logger.LogError("Непридвиденная внутренняя ошибка: {message}", ex.Message);
                throw new Exception("Непридвиденная внутренняя ошибка");
            }
        }

        public async Task InsertFood(AdminFoodModel food)
        {
            _logger.LogInformation("Получена заявка на добавление нового блюда - '{Name}'", food.Food.Name);
            try
            {
                if ((food.FrontPhoto is { Length: > 0 } frontPhoto) && (food.BackPhoto is { Length: > 0 } backPhoto))
                {
                    // Получение пути для front-изображения
                    string frontExtension = Path.GetExtension(frontPhoto.FileName);
                    string frontFileName = (food.Food.Name + "_front").Replace(" ", "_") + frontExtension;
                    var frontPath = Path.Combine("wwwroot/IMG/Food", frontFileName);

                    // Получение пути для back-изображения
                    string backExtension = Path.GetExtension(backPhoto.FileName);
                    string backFileName = (food.Food.Name + "_back").Replace(" ", "_") + backExtension;
                    var backPath = Path.Combine("wwwroot/IMG/Food", backFileName);

                    food.Food.FrontImageAddress = "/IMG/Food/" + frontFileName;
                    food.Food.BackImageAddress = "/IMG/Food/" + backFileName;

                    using var fStream = new FileStream(frontPath, FileMode.Create);
                    await frontPhoto.CopyToAsync(fStream);

                    using var bStream = new FileStream(backPath, FileMode.Create);
                    await backPhoto.CopyToAsync(bStream);

                    int categoryId = await _connection.QueryFirstAsync<int>(@"SELECT category_id FROM public.categories
                    WHERE name = @Name", new { Name = food.SelectedCategory });

                    await _connection.ExecuteAsync(@"INSERT INTO public.food 
                    (food_name, price, calories, weight, ingredients, front_image_address, back_image_address, category_id, description)
                    VALUES (@Name, @Price, @Calories, @Weight, @Ingredients, @FrontImageAddress, @BackImageAddress, @CategoryId, @Description);",
                        new
                        {
                            food.Food.Name,
                            food.Food.Price,
                            food.Food.Calories,
                            food.Food.Weight,
                            food.Food.Ingredients,
                            food.Food.FrontImageAddress,
                            food.Food.BackImageAddress,
                            categoryId,
                            food.Food.Description
                        });

                    _logger.LogInformation("Блюдо '{Name}' добавлено в базу данных", food.Food.Name);
                }
                else
                {
                    _logger.LogInformation("Ошибка при получении изображения для блюда '{Name}'", food.Food.Name);
                    throw new ArgumentNullException("Ошибка при получении изображения");
                }
            }
            catch (PostgresException ex) when (ex.Message.Contains("значение ключа нарушает ограничение уникальности"))
            {
                _logger.LogInformation("Блюдо '{Name}' уже существует", food.Food.Name);
                throw new ArgumentException("Такое блюдо уже существует");
            }
            catch (PostgresException ex)
            {
                _logger.LogError("Внутренняя ошибка в базе данных: {message}", ex.Message);
                throw new Exception("Внутренняя ошибка в базе данных");
            }
            catch (Exception ex)
            {
                _logger.LogError("Непридвиденная внутренняя ошибка: {message}", ex.Message);
                throw new Exception("Непридвиденная внутренняя ошибка");
            }
        }

        public async Task InsertOffer(AdminOfferModel offer)
        {
            _logger.LogInformation("Получена заявка на создание новой акции - '{Name}'", offer.Offer.Name);
            string sqlInsertOffer = @"INSERT INTO public.offers (offer_name, description, discount, starts_at, ends_at) 
                VALUES (@Name, @Description, @Discount, @StartsAt, @EndsAt) 
                RETURNING offer_id AS id;";

            decimal discountValue = offer.Offer.Discount / 100.0m;

            try
            {
                int offerId = await _connection.ExecuteScalarAsync<int>(sqlInsertOffer, new
                {
                    offer.Offer.Name,
                    offer.Offer.Description,
                    Discount = discountValue,
                    offer.Offer.StartsAt,
                    offer.Offer.EndsAt
                });

                _logger.LogInformation("Акция '{Name}' c {Id} добавлена в базу данных", offer.Offer.Name, offerId);

                if (offer.FoodIds != null && offer.FoodIds.Any())
                {
                    var valuesList = new List<string>();
                    var parameters = new DynamicParameters();

                    for (int i = 0; i < offer.FoodIds.Count; i++)
                    {
                        valuesList.Add($"(@FoodId{i}, @OfferId)");
                        parameters.Add($"@FoodId{i}", offer.FoodIds[i]);
                    }
                    parameters.Add("@OfferId", offerId);

                    string sqlOfferFood = $@"INSERT INTO public.offers_food (food_id, offer_id) 
                                 VALUES {string.Join(", ", valuesList)}";

                    await _connection.ExecuteAsync(sqlOfferFood, parameters);
                }
                _logger.LogInformation("Блюда для акции '{Name}' добавлены в базу данных", offer.Offer.Name);
            }
            catch (PostgresException ex)
            {
                _logger.LogError("Внутренняя ошибка в базе данных: {message}", ex.Message);
                throw new Exception("Внутренняя ошибка в базе данных");
            }
            catch (Exception ex)
            {
                _logger.LogError("Непридвиденная внутренняя ошибка: {message}", ex.Message);
                throw new Exception("Непридвиденная внутренняя ошибка");
            }
        }

        public async Task InsertPromocode(Promocode promocode)
        {
            _logger.LogInformation("Получена заявка на добавление промокода '{Code}'", promocode.Code);

            string sql = @"INSERT INTO public.promocodes (code, from_sum, discount, expires_at) 
                VALUES (@Code, @FromSum, @Discount, @ExpiresAt)";
            try
            {
                await _connection.ExecuteAsync(sql, new
                {
                    promocode.Code,
                    promocode.FromSum,
                    promocode.Discount,
                    promocode.ExpiresAt
                });

                _logger.LogInformation("Промокод '{Code}' добавлен в базу данных", promocode.Code);
            }
            catch (PostgresException ex) when (ex.Message.Contains("значение ключа нарушает ограничение уникальности"))
            {
                _logger.LogInformation("Промокод '{Code}' уже существует в базе данных", promocode.Code);
                throw new ArgumentException("Такой промокод уже существует");
            }
            catch (PostgresException ex)
            {
                _logger.LogError("Внутренняя ошибка в базе данных: {message}", ex.Message);
                throw new Exception("Внутренняя ошибка в базе данных");
            }
            catch (Exception ex)
            {
                _logger.LogError("Непридвиденная внутренняя ошибка: {message}", ex.Message);
                throw new Exception("Непридвиденная внутренняя ошибка");
            }
        }
    }
}
