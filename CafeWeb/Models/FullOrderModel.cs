namespace CafeWeb.Models
{
    public class FullOrderModel
    {
        public int UserId { get; set; }
        public string Login { get; set; } = null!;
        public Order Order { get; set; } = null!;
    }
}
