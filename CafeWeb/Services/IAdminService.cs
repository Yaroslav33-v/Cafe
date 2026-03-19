using CafeWeb.Models;

namespace CafeWeb.Services
{
    public interface IAdminService
    {
        Task InsertFood(AdminFoodModel food);
        Task InsertOffer(AdminOfferModel offer);
        Task InsertPromocode(Promocode promocode);
        Task InsertAdmin(User user);
        Task<List<string>> GetCategoryNames();
        Task<Dictionary<int, FoodShortModel>> GetAllFood();
    }
}
