using System.ComponentModel.DataAnnotations.Schema;

namespace TelegramBot.Entities
{
    internal class Promocode
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public decimal FromSum { get; set; }
        public decimal Discount { get; set; }
        public DateOnly ExpiresAt { get; set; }
    }
}
