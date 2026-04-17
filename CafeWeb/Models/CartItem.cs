namespace CafeWeb.Models
{
    public class CartItem
    {
        public Food Food { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal Total => Quantity * Food.Price;
    }
}
