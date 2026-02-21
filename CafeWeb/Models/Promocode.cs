namespace CafeWeb.Models
{
    public class Promocode
    {
        public string Code { get; set; } = null!; // уникальный (в сервисе через try/catch)
        public decimal FromSum { get; set; }
        public decimal Discount { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
