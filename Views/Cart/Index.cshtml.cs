using BoutiqueElegance.Models;
using BoutiqueElegance.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoutiqueElegance.Views.Cart
{
    public class IndexModel : PageModel
    {
        private readonly CartService _cartService;

        public IndexModel(CartService cartService)
        {
            _cartService = cartService;
        }

        public Models.Cart Cart { get; set; } = new Models.Cart();

        public async Task OnGetAsync()
        {
            Cart = await _cartService.GetCartAsync();
        }

        public async Task OnGetRefreshCartAsync()
        {
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

