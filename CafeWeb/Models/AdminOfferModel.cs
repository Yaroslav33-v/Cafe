namespace CafeWeb.Models
{
    public class AdminOfferModel // исользовать эту модель для /Admin/NewOffer
    {
        public Offer Offer { get; set; } = null!;
        public Dictionary<int, string> Foods { get; set; } = [];
        public List<int> FoodIds { get; set; } = [];
    }
}
