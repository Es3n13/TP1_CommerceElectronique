using System.Security.Claims;
using BoutiqueElegance.Data;
using BoutiqueElegance.Models;
using BoutiqueElegance.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace BoutiqueElegance.Pages.Checkout
{
    [Authorize(Roles = "Client")]
    public class IndexModel : PageModel
    {
        private readonly CartService _cartService;
        private readonly ApplicationDbContext _context;

        public IndexModel(CartService cartService, ApplicationDbContext context)
        {
            _cartService = cartService;
            _context = context;
        }

        public BoutiqueElegance.Models.Cart Cart { get; set; } = new BoutiqueElegance.Models.Cart();

        public decimal Total => Cart.Items.Sum(i => i.UnitPrice * i.Quantity);

        [BindProperty]
        public string CardHolderName { get; set; } = string.Empty;

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync()
        {
            Cart = await _cartService.GetCartAsync();
            if (!Cart.Items.Any())
                return RedirectToPage("/Cart/Index");

            Email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Cart = await _cartService.GetCartAsync();
            if (!Cart.Items.Any())
                return RedirectToPage("/Cart/Index");

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var client = await _context.Users.FirstAsync(u => u.Id == userId);

            // Créer un PaymentIntent Stripe (sandbox)
            var amountInCents = (long)(Total * 100);

            var paymentIntentService = new PaymentIntentService();
            var createOptions = new PaymentIntentCreateOptions
            {
                Amount = amountInCents,
                Currency = "cad",
                PaymentMethodTypes = new List<string> { "card" },
                ReceiptEmail = Email
            };

            var intent = await paymentIntentService.CreateAsync(createOptions);

            // Créer la commande
            var firstPlat = await _context.Plats
                .Include(p => p.Restaurant)
                .FirstAsync(p => p.Id == Cart.Items.First().PlatId);

            var order = new Order
            {
                ClientId = client.Id,
                RestaurantId = firstPlat.RestaurantId,
                CreatedAt = DateTime.UtcNow,
                TotalAmount = Total,
                Status = "Paid",
                StripePaymentIntentId = intent.Id
            };

            foreach (var item in Cart.Items)
            {
                order.Items.Add(new OrderItem
                {
                    PlatId = item.PlatId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                });
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Créer la facture
            var invoice = new BoutiqueElegance.Models.Invoice
            {
                OrderId = order.Id,
                TotalAmount = order.TotalAmount,
                CreatedAt = DateTime.UtcNow
            };
            _context.Invoices.Add(invoice);

            // Vider le panier
            _context.CartItems.RemoveRange(Cart.Items);
            _context.Carts.Remove(Cart);

            await _context.SaveChangesAsync();

            return RedirectToPage("/Orders/Details", new { id = order.Id });
        }
    }
}
