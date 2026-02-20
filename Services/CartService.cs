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
            var plat = await _context.Plats.FindAsync(platId);
            if (plat == null) return;

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
