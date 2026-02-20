using BoutiqueElegance.Models;
using BoutiqueElegance.Services;
using Microsoft.AspNetCore.Mvc;

namespace BoutiqueElegance.Controllers
{
    public class CartController : Controller
    {
        private readonly CartService _cartService;
        private readonly ILogger<CartController> _logger;

        public CartController(CartService cartService, ILogger<CartController> logger)
        {
            _cartService = cartService;
            _logger = logger;
        }

        // GET: /Cart/Index
        public async Task<IActionResult> Index()
        {
            var cart = await _cartService.GetCartAsync();
            return View(cart);
        }

        // POST: /Cart/AddToCart
        [HttpPost]
        public async Task<IActionResult> AddToCart(int platId)
        {
            try
            {
                await _cartService.AddToCartAsync(platId);

                // Succès - rediriger vers le panier
                return RedirectToAction("Index");
            }
            catch (RestaurantConflictException ex)
            {
                // Conflit détecté
                TempData["ConflictMessage"] = $"Vous avez des articles du restaurant {ex.CurrentRestaurantName}. " +
                    $"Vous essayez d'ajouter un article du restaurant {ex.NewRestaurantName}.";
                TempData["CurrentRestaurant"] = ex.CurrentRestaurantName;
                TempData["NewRestaurant"] = ex.NewRestaurantName;
                TempData["PlatId"] = platId;
                TempData["ShowConflict"] = true;

                var cart = await _cartService.GetCartAsync();
                return View("Index", cart);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur lors de l'ajout au panier : {ex.Message}");
                return RedirectToAction("Index");
            }
        }

        // POST: /Cart/KeepCart
        [HttpPost]
        public async Task<IActionResult> KeepCart()
        {
            return RedirectToAction("Index");
        }

        // POST: /Cart/ClearAndAdd
        [HttpPost]
        public async Task<IActionResult> ClearAndAdd(int platId)
        {
            try
            {
                await _cartService.ClearCartAsync();
                await _cartService.AddToCartAsync(platId);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur lors du remplacement du panier : {ex.Message}");
                return RedirectToAction("Index");
            }
        }

        // POST: /Cart/RemoveItem
        [HttpPost]
        public async Task<IActionResult> RemoveItem(int itemId)
        {
            var cart = await _cartService.GetCartAsync();

            if (cart?.Items != null)
            {
                var itemToRemove = cart.Items.FirstOrDefault(i => i.Id == itemId);
                if (itemToRemove != null)
                {
                    cart.Items.Remove(itemToRemove);
                    // Sauvegarder si nécessaire
                }
            }

            return RedirectToAction("Index");
        }

        // POST: /Cart/ClearCart
        [HttpPost]
        public async Task<IActionResult> ClearCart()
        {
            await _cartService.ClearCartAsync();
            return RedirectToAction("Index");
        }

        // Méthode helper pour calculer le total
        public decimal GetTotal(Cart cart)
        {
            if (cart?.Items == null || !cart.Items.Any())
                return 0;
            return cart.Items.Sum(i => i.UnitPrice * i.Quantity);
        }
    }
}
