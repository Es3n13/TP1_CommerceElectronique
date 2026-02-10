using BoutiqueElegance.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoutiqueElegance.Pages.Cart
{
    public class AddModel : PageModel
    {
        private readonly CartService _cartService;

        public AddModel(CartService cartService)
        {
            _cartService = cartService;
        }

        public async Task<IActionResult> OnGetAsync(int platId, int restaurantId)
        {
            await _cartService.AddToCartAsync(platId);

            // Retour à la page du restaurant
            return RedirectToPage("/Restaurants/Details", new { id = restaurantId });
        }
    }
}
