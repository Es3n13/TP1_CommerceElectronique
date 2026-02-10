

namespace BoutiqueElegance.Models
{
    public class Restaurant
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string ImageUrl { get; set; }
        public int? SellerId { get; set; }
        public User? Seller { get; set; }

        public ICollection<RestaurantTag> RestaurantTags { get; set; }
        public ICollection<Plat> Plats { get; set; }
    }
}
