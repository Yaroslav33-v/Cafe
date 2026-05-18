using CafeWeb.Models;

namespace CafeWeb.Services
{
    public interface ICartService
    {
        Cart GetCart();
        void AddToCart(Food item);
        void AddOfferToCart(OfferCartItem item);
        void RemoveFromCart(int menuItemId);
        void RemoveOfferFromCart(int offerItemId);
        void UpdateQuantity(int menuItemId, int quantity);
        void UpdateOfferQuantity(int offerItemId, int quantity);
        void ClearCart();
    }
}
