namespace BoutiqueElegance.Models
{
    public class RestaurantTag
    {
        public int RestaurantId { get; set; }
        public Restaurant Restaurant { get; set; }

        public int TagId { get; set; }
        public Tag Tag { get; set; }
    }
}
