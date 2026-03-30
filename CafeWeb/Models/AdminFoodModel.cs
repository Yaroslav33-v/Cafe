namespace CafeWeb.Models
{
    public class AdminFoodModel
    {
        public List<string> Categories { get; set; } = null!;
        public string SelectedCategory { get; set; } = null!;
        public IFormFile FrontPhoto { get; set; } = null!;
        public IFormFile BackPhoto { get; set; } = null!;
        public Food Food { get; set; } = null!;
        public Dictionary<int, FoodShortModel> ExistingFood { get; set; } = null!;
    }
}
