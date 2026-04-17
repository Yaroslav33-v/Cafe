namespace CafeWeb.Models
{
    public class Cart
    {
        public List<CartItem> Items { get; set; } = [];
        public decimal TotalAmount => Items.Sum(i => i.Total);
        public int TotalItems => Items.Sum(i => i.Quantity);
    }
}
