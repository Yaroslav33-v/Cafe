namespace CafeWeb.Models
{
    public class PaymentModel
    {
        public string CardNumber { get; set; } = null!;
        public string MonthYear { get; set; } = null!;
        public string CvvCode { get; set; } = null!;
        public decimal TotalAmount { get; set; }
    }
}
