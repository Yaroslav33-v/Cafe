namespace CafeWeb.Models
{
    public class OfferCartItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public List<Food> Foods { get; set; } = [];
        public decimal Discount { get; set; }
        public int Quantity { get; set; }
        public decimal Total => Quantity * (Foods.Sum(f => f.Price) * (1 - Discount));
    }
}
