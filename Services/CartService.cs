using System.Text.Json;
using BoutiqueElegance.Data;
using BoutiqueElegance.Models;
using Microsoft.EntityFrameworkCore;

namespace BoutiqueElegance.Services
{
    public class CartService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string CartIdSessionKey = "CartId";

        public CartService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private ISession Session => _httpContextAccessor.HttpContext!.Session;

        private async Task<Cart> GetOrCreateCartAsync()
        {
            int? cartId = Session.GetInt32(CartIdSessionKey);

            Cart? cart = null;

            if (cartId.HasValue)
            {
                cart = await _context.Carts
                    .Include(p => p.Items)
                        .ThenInclude(i => i.Plat)
                    .FirstOrDefaultAsync(p => p.Id == cartId.Value);
            }

            if (cart == null)
            {
                cart = new Cart();
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
                Session.SetInt32(CartIdSessionKey, cart.Id);
            }

            return cart;
        }

        public async Task AddToCartAsync(int platId, int quantity)
        {
            if (quantity < 1) quantity = 1;

            var cart = await GetOrCreateCartAsync();

            // Charger le plat avec son restaurant
            var plat = await _context.Plats
                .Include(p => p.Restaurant)
                .FirstOrDefaultAsync(p => p.Id == platId);

            if (plat == null) return;

            // S'il y a déjà des articles dans le panier, vérifier le resto du 1er plat
            if (cart.Items != null && cart.Items.Any())
            {
                var firstPlatId = cart.Items.First().PlatId;

                var firstPlat = await _context.Plats
                    .Include(p => p.Restaurant)
                    .FirstOrDefaultAsync(p => p.Id == firstPlatId);

                if (firstPlat != null &&
                    firstPlat.Restaurant?.Id != plat.Restaurant?.Id)
                {
                    // Si conflit on leve l'exception
                    throw new RestaurantConflictException(
                        currentRestaurantId: firstPlat.Restaurant.Id,
                        currentRestaurantName: firstPlat.Restaurant.Name,
                        newRestaurantId: plat.Restaurant.Id,
                        newRestaurantName: plat.Restaurant.Name
                    );
                }
            }

            // Pas de conflit = ajout normal
            var existingItem = cart.Items.FirstOrDefault(i => i.PlatId == platId);

            if (existingItem == null)
            {
                cart.Items.Add(new CartItem
                {
                    PlatId = platId,
                    Quantity = quantity,
                    UnitPrice = plat.Price
                });
            }
            else
            {
                existingItem.Quantity += quantity;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<Cart> GetCartAsync()
        {
            return await GetOrCreateCartAsync();
        }

        public async Task RemoveFromCartAsync(int cartItemId)
        {
            var item = await _context.CartItems.FindAsync(cartItemId);
            if (item != null)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
            }
        }

        public async Task ClearCartAsync()
        {
            var cart = await GetOrCreateCartAsync();

            if (cart?.Items != null)
            {
                cart.Items.Clear();
                await _context.SaveChangesAsync();
            }
        }
    }
}
