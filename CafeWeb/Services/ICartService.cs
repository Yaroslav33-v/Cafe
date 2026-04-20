using CafeWeb.Models;

namespace CafeWeb.Services
{
    public interface ICartService
    {
        Cart GetCart();
        void AddToCart(Food item);
        void RemoveFromCart(int menuItemId);
        void UpdateQuantity(int menuItemId, int quantity);
        void ClearCart();
    }
}
