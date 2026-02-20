using CafeWeb.Models;
using Dapper;
using Npgsql;
using System.Data;

namespace CafeWeb.Services
{
    public class AdminService : IAdminService
    {
        private readonly IDbConnection _connection;

        public AdminService(IDbConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
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

                    return (true, null);
                }

                return (false, "Ошибка при получении изображения");
            }
            catch (PostgresException ex) when (ex.Message.Contains("значение ключа нарушает ограничение уникальности"))
            {
                Console.WriteLine(ex.Message);
                return (false, "Такое блюдо уже существует");
            }
            catch (PostgresException ex)
            {
                return (false, "Внутренняя ошибка в базе данных");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return (false, "Непридвиденная внутренняя ошибка");
            }
        }

        public async Task<(bool, string?)> InsertOffer(AdminOfferModel offer)
        {
            try
            {
                return(true, null);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return (false, "Непридвиденная внутренняя ошибка");
            }
        }

        public async Task<(bool, string?)> InsertPromocode(Promocode promocode)
        {
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

                return (true, null);
            }
            catch (PostgresException ex) when (ex.Message.Contains("значение ключа нарушает ограничение уникальности"))
            {
                Console.WriteLine(ex.Message);
                return (false, "Такой промокод уже существует");
            }
            catch (PostgresException ex)
            {
                return (false, "Внутренняя ошибка в базе данных");
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.Message);
                return (false, "Непридвиденная внутренняя ошибка");
            }
        }
    }
}
