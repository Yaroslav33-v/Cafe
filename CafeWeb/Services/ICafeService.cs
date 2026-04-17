using CafeWeb.Models;

namespace CafeWeb.Services
{
    public interface ICafeService
    {
        Task<List<Category>> GetFoods();
        Task<Category?> GetFavourites(int userId);
        Task<Food> GetFood(int foodId);
    }
}
