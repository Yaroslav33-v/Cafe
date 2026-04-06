using CafeWeb.Models;

namespace CafeWeb.Services
{
    public interface IPromocodeService
    {
        Task<Promocode?> GetPromocodeInfo(string code);
    }
}
