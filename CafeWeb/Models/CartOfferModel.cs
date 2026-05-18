namespace CafeWeb.Models
{
    public class CartOfferModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Discount { get; set; }
        public List<int> FoodIds { get; set; } = [];
    }
}
