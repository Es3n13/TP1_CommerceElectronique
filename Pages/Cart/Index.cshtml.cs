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
            // Toujours charger le panier frais depuis la base de données/session
            Cart = await _cartService.GetCartAsync();
        }

        /// <summary>
        /// Handler spécial pour rafraîchir le panier après ajout d'articles
        /// </summary>
        public async Task OnGetRefreshCartAsync()
        {
            // Charger le panier - c'est la même chose que OnGetAsync
            // Mais cette méthode garantit un rechargement complet
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
            // Charger le panier actuel
            Cart = await _cartService.GetCartAsync();
            
            // Trouver et supprimer l'article
            if (Cart.Items != null)
            {
                var itemToRemove = Cart.Items.FirstOrDefault(i => i.Id == itemId);
                if (itemToRemove != null)
                {
                    Cart.Items.Remove(itemToRemove);
                    
                    // Mettre à jour la session du panier
                    await _cartService.SaveCartAsync(Cart);
                }
            }
            
            // Retourner JSON indiquant succès
            return new JsonResult(new { success = true });
        }
    }
}

