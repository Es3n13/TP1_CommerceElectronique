using System.Security.Claims;
using BoutiqueElegance.Data;
using BoutiqueElegance.Models;
using BoutiqueElegance.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace BoutiqueElegance.Controllers
{
    [Authorize(Roles = "Client")]
    public class CheckoutController : Controller
    {
        private readonly CartService _cartService;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public CheckoutController(CartService cartService, ApplicationDbContext context, IConfiguration config)
        {
            _cartService = cartService;
            _context = context;
            _config = config;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var cart = await _cartService.GetCartAsync();
            if (!cart.Items.Any())
                return RedirectToAction("Index", "Cart");

            // Récupérer l'email de l'utilisateur connecté
            var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

            // Calculer le total
            var total = cart.Items.Sum(i => i.UnitPrice * i.Quantity);

            // Créer le PaymentIntent Stripe
            StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];
            var amountInCents = (long)(total * 100);
            var service = new PaymentIntentService();
            var options = new PaymentIntentCreateOptions
            {
                Amount = amountInCents,
                Currency = "cad",
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true
                },
                ReceiptEmail = email
            };
            var intent = await service.CreateAsync(options);

            // Passer les données à la vue via ViewBag
            ViewBag.Cart = cart;
            ViewBag.Total = total;
            ViewBag.PublicKey = _config["Stripe:PublicKey"];
            ViewBag.ClientSecret = intent.ClientSecret;
            ViewBag.Email = email;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string paymentIntentId)
        {
            var cart = await _cartService.GetCartAsync();
            if (!cart.Items.Any())
                return RedirectToAction("Index", "Cart");

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var client = await _context.Users.FirstAsync(u => u.Id == userId);

            // Vérifier le PaymentIntent côté Stripe
            StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];
            var service = new PaymentIntentService();
            var intent = await service.GetAsync(paymentIntentId);

            if (intent.Status != "succeeded")
            {
                ModelState.AddModelError(string.Empty, "Le paiement n'a pas été confirmé.");
                return await Index();
            }

            // Calculer le total
            var total = cart.Items.Sum(i => i.UnitPrice * i.Quantity);

            // Créer la commande
            var firstPlat = await _context.Plats
                .Include(p => p.Restaurant)
                .FirstAsync(p => p.Id == cart.Items.First().PlatId);

            var order = new Order
            {
                ClientId = client.Id,
                RestaurantId = firstPlat.RestaurantId,
                CreatedAt = DateTime.UtcNow,
                TotalAmount = total,
                Status = "Paid",
                StripePaymentIntentId = intent.Id
            };

            foreach (var item in cart.Items)
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
            _context.CartItems.RemoveRange(cart.Items);
            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Orders", new { id = order.Id });
        }
    }
}

