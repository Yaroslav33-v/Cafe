namespace TelegramBot.Entities
{
    internal class Offer
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Discount { get; set; }
        public DateOnly StartsAt { get; set; }
        public DateOnly EndsAt { get; set; }
        public bool IsNotificated { get; set; }
    }
}
