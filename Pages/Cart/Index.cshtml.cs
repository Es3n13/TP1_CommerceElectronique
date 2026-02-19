using BoutiqueElegance.Models;
using BoutiqueElegance.Services;
using Microsoft.AspNetCore.Mvc;
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

        public async Task OnGetRefreshCartAsync()
        {
            // Toujours charger le panier frais depuis la base de données/session
            Cart = await _cartService.GetCartAsync();
        }

        public decimal GetTotal()
        {
            if (Cart.Items == null || !Cart.Items.Any())
                return 0;

            return Cart.Items.Sum(i => i.UnitPrice * i.Quantity);
        }

        public async Task<IActionResult> OnPostDeleteAsync(int itemId)
        {
            await _cartService.RemoveFromCartAsync(itemId);
            return RedirectToPage();
        }
    }
}

