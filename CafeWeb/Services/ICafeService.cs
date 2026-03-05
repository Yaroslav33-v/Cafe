using CafeWeb.Models;

namespace CafeWeb.Services
{
    public interface ICafeService
    {
        Task<List<Category>> GetFoods();

    }
}
