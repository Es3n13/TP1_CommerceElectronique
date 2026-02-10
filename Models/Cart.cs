namespace BoutiqueElegance.Models
{
    public class Cart
    {
        public int Id { get; set; }

        // On mva ClientId quand l’auth sera prête
        // public int ClientId { get; set; }
        // public User Client { get; set; }

        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }

    public class CartItem
    {
        public int Id { get; set; }

        public int CartId { get; set; }
        public Cart Cart { get; set; }

        public int PlatId { get; set; }
        public Plat Plat { get; set; }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
