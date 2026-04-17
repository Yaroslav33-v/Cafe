using CafeWeb.Models;
using CafeWeb.Static;

namespace CafeWeb.Services
{
    public class CartService : ICartService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string CartSessionKey = "CafeCart";
        private ISession Session => _httpContextAccessor.HttpContext.Session;

        public CartService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public Cart GetCart()
        {
            var cart = Session.GetObject<Cart>(CartSessionKey);
            if (cart == null)
            {
                cart = new Cart();
                Session.SetObject(CartSessionKey, cart);
            }
            return cart;
        }

        public void AddToCart(Food item)
        {
            var cart = GetCart();
            var existingItem = cart.Items.FirstOrDefault(i => i.Food.Id == item.Id);

            if (existingItem != null)
                existingItem.Quantity++;
            else
            {
                cart.Items.Add(new CartItem
                {
                    Food = item,
                    Quantity = 1
                });
            }

            Session.SetObject(CartSessionKey, cart);
        }

        public void RemoveFromCart(int menuItemId)
        {
            var cart = GetCart();
            cart.Items.RemoveAll(i => i.Food.Id == menuItemId);
            Session.SetObject(CartSessionKey, cart);
        }

        public void UpdateQuantity(int menuItemId, int quantity)
        {
            var cart = GetCart();
            var item = cart.Items.FirstOrDefault(i => i.Food.Id == menuItemId);

            if (item != null)
            {
                if (quantity <= 0)
                    cart.Items.Remove(item);
                else
                    item.Quantity = quantity;

                Session.SetObject(CartSessionKey, cart);
            }
        }

        public void ClearCart() => Session.Remove(CartSessionKey);
    }
}
