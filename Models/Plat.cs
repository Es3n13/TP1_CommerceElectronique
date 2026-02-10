namespace BoutiqueElegance.Models
{
    public class Plat
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } //Facultatif (peut-être même à enlever)
        public string Category { get; set; } // Entrée/Plat principal/Dessert

        public int RestaurantId { get; set; }
        public Restaurant Restaurant { get; set; }
    }
}
