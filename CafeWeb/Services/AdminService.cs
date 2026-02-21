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

        public AdminService(IDbConnection connection, ILogger<AdminService> logger)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
         
        public async Task<Dictionary<int, string>> GetAllFood()
        {
            IEnumerable<(int id, string name)> foods = await _connection
                .QueryAsync<(int, string)>("SELECT food_id AS Id, food_name AS Name FROM public.food");

            return foods.ToDictionary();
        }

        public async Task<List<string>> GetCategoryNames()
        {
            IEnumerable<string> categoryNames = await _connection.QueryAsync<string>("SELECT name FROM public.categories");

            return categoryNames.ToList();
        }

        public async Task<(bool, string?)> InsertFood(AdminFoodModel food)
        {
            _logger.LogInformation("Получена заявка на добавление нового блюда - '{Name}'", food.Food.Name);
            try
            {
                if (food.Photo is { Length: > 0 } photo)
                {
                    string extension = Path.GetExtension(photo.FileName);
                    string fileName = food.Food.Name.Replace(" ", "_") + extension;
                    var path = Path.Combine("wwwroot/IMG", fileName);

                    food.Food.ImageAddress = "/IMG/" + fileName;

                    using var stream = new FileStream(path, FileMode.Create);
                    await photo.CopyToAsync(stream);

                    int categoryId = await _connection.QueryFirstAsync<int>(@"SELECT category_id FROM public.categories
                    WHERE name = @Name", new { Name = food.SelectedCategory });

                    await _connection.ExecuteAsync(@"INSERT INTO public.food 
                    (food_name, price, calories, weight, ingredients, image_address, category_id)
                    VALUES (@Name, @Price, @Calories, @Weight, @Ingredients, @ImageAddress, @CategoryId);",
                        new
                        {
                            food.Food.Name,
                            food.Food.Price,
                            food.Food.Calories,
                            food.Food.Weight,
                            food.Food.Ingredients,
                            food.Food.ImageAddress,
                            categoryId
                        });

                    _logger.LogInformation("Блюдо '{Name}' добавлено в базу данных", food.Food.Name);
                    return (true, null);
                }

                _logger.LogInformation("Ошибка при получении изображения для блюда '{Name}'", food.Food.Name);
                return (false, "Ошибка при получении изображения");
            }
            catch (PostgresException ex) when (ex.Message.Contains("значение ключа нарушает ограничение уникальности"))
            {
                _logger.LogInformation("Блюдо '{Name}' уже существует", food.Food.Name);
                return (false, "Такое блюдо уже существует");
            }
            catch (PostgresException ex)
            {
                _logger.LogError("Внутренняя ошибка в базе данных: " + ex.Message);
                return (false, "Внутренняя ошибка в базе данных");
            }
            catch (Exception ex)
            {
                _logger.LogError("Непридвиденная внутренняя ошибка: " + ex.Message);
                return (false, "Непридвиденная внутренняя ошибка");
            }
        }

        public async Task<(bool, string?)> InsertOffer(AdminOfferModel offer)
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

                return (true, null);
            }
            catch (PostgresException ex)
            {
                _logger.LogError("Внутренняя ошибка в базе данных: " + ex.Message);
                return (false, "Внутренняя ошибка в базе данных");
            }
            catch (Exception ex)
            {
                _logger.LogError("Непридвиденная внутренняя ошибка: " + ex.Message);
                return (false, "Непридвиденная внутренняя ошибка");
            }
        }

        public async Task<(bool, string?)> InsertPromocode(Promocode promocode)
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
                return (true, null);
            }
            catch (PostgresException ex) when (ex.Message.Contains("значение ключа нарушает ограничение уникальности"))
            {
                _logger.LogInformation("Промокод '{Code}' уже существует в базе данных", promocode.Code);
                return (false, "Такой промокод уже существует");
            }
            catch (PostgresException ex)
            {
                _logger.LogError("Внутренняя ошибка в базе данных: " + ex.Message);
                return (false, "Внутренняя ошибка в базе данных");
            }
            catch (Exception ex)
            {
                _logger.LogError("Непридвиденная внутренняя ошибка: " + ex.Message);
                return (false, "Непридвиденная внутренняя ошибка");
            }
        }
    }
}
