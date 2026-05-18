using CafeWeb.Models;
using Dapper;
using Npgsql;
using System.Data;

namespace CafeWeb.Services
{
    public class CafeService : ICafeService
    {
        private readonly IDbConnection _connection;
        private readonly ILogger<CafeService> _logger;

        public CafeService(IDbConnection connection, ILogger<CafeService> logger)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }

        public async Task<Category?> GetFavourites(int userId)
        {
            try
            {
                // Получаем блюда из избранного
                var favFoods = await _connection
                    .QueryAsync<Food>(@"
                        SELECT 
                            f.food_id AS Id,
                            f.food_name AS Name,
                            f.price,
                            f.calories,
                            f.weight,
                            f.ingredients,
                            f.description,
                            f.front_image_address AS FrontImageAddress,
                            f.back_image_address AS BackImageAddress
                        FROM public.favourite_food ff
                        JOIN public.food f ON ff.food_id = f.food_id
                        LEFT JOIN public.categories c ON f.category_id = c.category_id
                        WHERE ff.user_id = @userId",
                        new
                        {
                            userId
                        });

                if (!favFoods.Any())
                    throw new Exception();

                // Возвращаем избранное как категорию
                return new Category
                {
                    Name = "Избранное",
                    Foods = favFoods.ToList()
                };
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<Category>> GetFoods()
        {
            try
            {
                // Получаем все категории
                var categories = (await _connection
                    .QueryAsync<Category>("SELECT category_id AS Id, name FROM public.categories"))
                    .ToList();

                // Получаем все блюда
                var foods = (await _connection
                    .QueryAsync<Food>(@"
                        SELECT 
                            food_id AS Id, 
                            food_name AS Name, 
                            price,
                            calories,
                            weight,
                            ingredients,
                            description,
                            front_image_address AS FrontImageAddress,
                            back_image_address AS BackImageAddress,
                            category_id AS CategoryId
                        FROM public.food"))
                    .ToList();

                // Группируем блюда по категориям
                foreach (var category in categories)
                    category.Foods = foods
                        .Where(f => f.CategoryId == category.Id)
                        .ToList();

                return categories;
            }
            catch (Exception ex)
            {
                _logger.LogError("Непридвиденная внутренняя ошибка: {message}", ex.Message);
                throw new Exception("Непредвиденная внутренняя ошибка");
            }
        }

        public async Task<Food> GetFood(int foodId)
        {
            try
            {
                var food = await _connection
                    .QueryFirstOrDefaultAsync<Food>(@"
                        SELECT 
                            food_id AS Id, 
                            food_name AS Name, 
                            price,
                            calories,
                            weight,
                            ingredients,
                            description,
                            front_image_address AS FrontImageAddress,
                            back_image_address AS BackImageAddress,
                            category_id AS CategoryId
                        FROM public.food
                        WHERE food_id = @FoodId",
                        new
                        {
                            FoodId = foodId
                        }) ?? throw new Exception("Не удалось получить блюдо");
                return food;
            }
            catch (Exception ex)
            {
                _logger.LogError("Не удалось получить блюдо: {message}", ex.Message);
                throw new Exception("Не удалось получить блюдо");
            }
        }

        public async Task UpdateFavourite(int foodId, int userId)
        {
            bool exists = await _connection.ExecuteScalarAsync<bool>(@"
                SELECT EXISTS(
                    SELECT 1 
                    FROM public.favourite_food 
                    WHERE food_id = @FoodId AND user_id = @UserId
                )", 
                new {
                    FoodId = foodId,
                    UserId = userId
                });

            if(exists)
                // Удаляем запись
                await _connection.ExecuteAsync(@"
                    DELETE FROM public.favourite_food 
                    WHERE food_id = @FoodId AND user_id = @UserId",
                    new { FoodId = foodId, UserId = userId });
            else
                // Добавляем запись
                await _connection.ExecuteAsync(@"
                    INSERT INTO public.favourite_food (food_id, user_id) 
                    VALUES (@FoodId, @UserId)",
                    new { FoodId = foodId, UserId = userId });
        }

        public async Task<List<Food>> GetFoodsByIds(List<int> foodIds)
        {
            try
            {
                if (foodIds is null || !foodIds.Any())
                    return [];

                var result = await _connection.QueryAsync<Food>(@"
                    SELECT 
                        food_id AS Id,
                        food_name AS Name,
                        price AS Price,
                        calories AS Calories,
                        weight AS Weight,
                        ingredients AS Ingredients,
                        description AS Description,
                        front_image_address AS FrontImageAddress,
                        back_image_address AS BackImageAddress,
                        category_id AS CategoryId
                    FROM public.food
                    WHERE food_id = ANY(@FoodIds)", new { FoodIds = foodIds });

                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении блюд по ID: {Message}", ex.Message);
                return [];
            }
        }
    }
}
