namespace CafeWeb.Models
{
    public class Food // Описание модели
    {
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public string Description { get; set; } = null!;
    }
}
