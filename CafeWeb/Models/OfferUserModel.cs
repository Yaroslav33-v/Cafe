namespace CafeWeb.Models
{
    public class OfferUserModel
    {
        public Offer Offer { get; set; } = null!;
        public List<Food> Foods { get; set; } = [];
    }
}
