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

        public async Task AddToCartAsync(int platId)
        {
            var plat = await _context.Plats.FindAsync(platId);
            if (plat == null) return;

            var cart = await GetOrCreateCartAsync();

            var existingItem = cart.Items.FirstOrDefault(i => i.PlatId == platId);

            if (existingItem == null)
            {
                var item = new CartItem
                {
                    PlatId = platId,
                    Quantity = 1,
                    UnitPrice = plat.Price
                };
                cart.Items.Add(item);
            }
            else
            {
                existingItem.Quantity += 1;
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
    }
}
