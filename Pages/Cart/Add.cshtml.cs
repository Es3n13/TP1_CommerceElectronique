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

        /// Ajoute un article au panier avec la quantité spécifiée
        public async Task<IActionResult> OnGetAsync(int platId, int restaurantId, int quantity = 1)
        {
            // Validation
            if (quantity < 1)
                quantity = 1;
            if (quantity > 99)
                quantity = 99;

            // Ajouter au panier la quantité spécifiée
            for (int i = 0; i < quantity; i++)
            {
                await _cartService.AddToCartAsync(platId);
            }

            // Retour à la page du restaurant
            return RedirectToPage("/Restaurants/Details", new { id = restaurantId });
        }
    }
}

