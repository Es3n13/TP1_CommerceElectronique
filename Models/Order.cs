

namespace BoutiqueElegance.Models
{
    public class Order
    {
        public int Id { get; set; }

        public int ClientId { get; set; }
        public User Client { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Paid, Failed

        public string StripePaymentIntentId { get; set; } = string.Empty;

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public Invoice? Invoice { get; set; }

        public int RestaurantId { get; set; }
        public Restaurant Restaurant { get; set; }
    }
}