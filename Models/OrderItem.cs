using BoutiqueElegance.Models;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; }

    public int PlatId { get; set; }
    public Plat Plat { get; set; }

    //public int SellerId { get; set; }
    //public User Seller { get; set; }

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
