using CafeWeb.Models;

namespace CafeWeb.Services
{
    public interface IAdminService
    {
        Task<(bool, string?)> InsertFood(AdminFoodModel food);
        Task<(bool, string?)> InsertOffer(AdminOfferModel offer);
        Task<(bool, string?)> InsertPromocode(Promocode promocode);
        Task<List<string>> GetCategoryNames();
        Task<Dictionary<int, string>> GetAllFood();
    }
}
