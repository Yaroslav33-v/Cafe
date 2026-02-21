namespace CafeWeb.Models
{
    public class Offer
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Discount { get; set; }
        public DateTime StartsAt { get; set; }
        public DateTime EndsAt { get; set; }
    }
}
