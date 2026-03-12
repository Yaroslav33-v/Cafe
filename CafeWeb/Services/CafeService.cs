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

        public async Task<List<Category>> GetFoods()
        {
            try
            {
                // Получаем все категории
                var categories = (await _connection
                    .QueryAsync<Category>("SELECT category_id AS Id, name AS Name FROM public.categories"))
                    .ToList();

                // Получаем все блюда
                var foods = (await _connection
                    .QueryAsync<Food>(@"
            SELECT 
                food_id AS Id, 
                food_name AS Name, 
                price AS Price,
                calories AS Calories,
                weight AS Weight,
                ingredients AS Ingredients,
                image_address AS ImageAddress,
                category_id AS CategoryId
            FROM public.food"))
                    .ToList();

                // Группируем блюда по категориям
                foreach (var category in categories)
                {
                    category.Foods = foods.Where(f => f.CategoryId == category.Id).ToList();
                }

                return categories;
            }
            catch (Exception ex)
            {
                _logger.LogError("Непридвиденная внутренняя ошибка: " + ex.Message);
                throw new Exception("Непредвиденная внутренняя ошибка");
            }
        }
    }
}
