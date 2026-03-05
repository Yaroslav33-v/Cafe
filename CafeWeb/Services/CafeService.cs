using CafeWeb.Models;
using Dapper;
using System.Data;

namespace CafeWeb.Services
{
    public class CafeService : ICafeService
    {
        private readonly IDbConnection _connection;

        public CafeService(IDbConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public async Task<List<Category>> GetFoods()
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
        public List<Category> GetFoods1()
        {

            // как реализовать sql запрос возвращающий json
            // получаем все из category
            // передаем в return

            List<Category> categories = [ 
                new Category{
                    Name = "яйца",
                    Foods = [
                        new Food{Name = "4 Творожка", Price = 1488},
                        new Food{Name = "Фа пепе шнейне", Price = 1337}
                    ]
                }
            ];
            return categories;
        }
    }
}
