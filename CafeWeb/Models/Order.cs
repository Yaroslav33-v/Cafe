namespace CafeWeb.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string Number { get; set; } = null!;
        public DateTime CreatedAt { get; set; } 
        public string Status { get; set; } = null!;
        public DateTime? DoneAt { get; set; }
        public List<CartItem> CartItems { get; set; } = [];
    }
}
