namespace CafeWeb.Models
{
    public class Offer
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Discount { get; set; }
        public DateOnly StartsAt { get; set; }
        public DateOnly EndsAt { get; set; }
    }
}
