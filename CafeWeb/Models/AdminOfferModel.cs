namespace CafeWeb.Models
{
    public class AdminOfferModel // исользовать эту модель для /Admin/NewOffer
    {
        public Offer Offer { get; set; } = null!;
        public List<Food> FoodInOffer { get; set; } = [];
    }
}
