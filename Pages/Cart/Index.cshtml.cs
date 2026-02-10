using BoutiqueElegance.Models;
using BoutiqueElegance.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoutiqueElegance.Pages.Cart
{
    public class IndexModel : PageModel
    {
        private readonly CartService _cartService;

        public IndexModel(CartService cartService)
        {
            _cartService = cartService;
        }

        public BoutiqueElegance.Models.Cart Cart { get; set; } = new BoutiqueElegance.Models.Cart();

        public async Task OnGetAsync()
        {
            Cart = await _cartService.GetCartAsync();
        }

        public decimal GetTotal()
        {
            return Cart.Items.Sum(i => i.UnitPrice * i.Quantity);
        }
    }
}
