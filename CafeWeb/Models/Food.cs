namespace CafeWeb.Models
{
    public class Food // Описание модели
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public decimal Calories { get; set; }
        public decimal Weight { get; set; }
        public string Ingredients { get; set; } = null!;
        public string ImageAddress { get; set; } = null!;
    }
}
