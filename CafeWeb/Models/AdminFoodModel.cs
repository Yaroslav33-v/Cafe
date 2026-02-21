namespace CafeWeb.Models
{
    public class AdminFoodModel
    {
        public List<string> Categories { get; set; } = null!;
        public string SelectedCategory { get; set; } = null!;
        public IFormFile Photo { get; set; } = null!;
        public Food Food { get; set; } = null!;
    }
}
