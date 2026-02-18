using BoutiqueElegance.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoutiqueElegance.Pages.Cart
{
    public class AddMultipleModel : PageModel
    {
        private readonly CartService _cartService;

        public AddMultipleModel(CartService cartService)
        {
            _cartService = cartService;
        }

   
        // Classe pour recevoir les articles du panier local
        public class CartItem
        {
            public int PlatId { get; set; }
            public int Quantity { get; set; }
        }

        // Ajoute plusieurs articles au panier en une seule requête
        public async Task<IActionResult> OnPostAsync(List<CartItem> items, int restaurantId)
        {
            // Validation des paramètres
            if (items == null || !items.Any())
            {
                return BadRequest("Aucun article dans le panier");
            }

            if (restaurantId <= 0)
            {
                return BadRequest("Restaurant invalide");
            }

            try
            {
                // Ajouter chaque article au panier
                foreach (var item in items)
                {
                    // Valider les données
                    if (item.PlatId <= 0)
                        continue;

                    int quantity = item.Quantity;

                    // Valider la quantité
                    if (quantity < 1)
                        quantity = 1;
                    if (quantity > 99)
                        quantity = 99;

                    // Ajouter l'article au panier la quantité spécifiée
                    for (int i = 0; i < quantity; i++)
                    {
                        await _cartService.AddToCartAsync(item.PlatId);
                    }
                }

                // Rediriger vers le panier avec un message de succès
                return RedirectToPage("/Cart/Index", new { success = true });
            }
            catch (Exception ex)
            {
                // Log l'erreur si nécessaire
                return BadRequest($"Erreur lors de l'ajout au panier: {ex.Message}");
            }
        }
    }
}


