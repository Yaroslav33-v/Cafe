using CafeWeb.Models;

namespace CafeWeb.Services
{
    public interface IAdminService
    {
        Task InsertFood(AdminFoodModel food);
        Task InsertOffer(Offer offer);
        Task<(bool, string?)> InsertPromocode(Promocode promocode);
        Task<List<string>> GetCategoryNames();
    }
}
