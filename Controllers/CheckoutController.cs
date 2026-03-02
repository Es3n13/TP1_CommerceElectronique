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
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(
            CartService cartService,
            ApplicationDbContext context,
            IConfiguration config,
            ILogger<CheckoutController> logger)
        {
            _cartService = cartService;
            _context = context;
            _config = config;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var cart = await _cartService.GetCartAsync();
            if (cart.Items == null || !cart.Items.Any())
                return RedirectToAction("Index", "Cart");

            var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
            var total = cart.Items.Sum(i => i.UnitPrice * i.Quantity);

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

            ViewBag.Cart = cart;
            ViewBag.Total = total;
            ViewBag.PublicKey = _config["Stripe:PublicKey"];
            ViewBag.ClientSecret = intent.ClientSecret;
            ViewBag.Email = email;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string paymentIntentId)
        {
            var cart = await _cartService.GetCartAsync();
            if (cart.Items == null || !cart.Items.Any())
                return RedirectToAction("Index", "Cart");

            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var client = await _context.Users.FirstAsync(u => u.Id == userId);

                // Vérifier le PaymentIntent côté Stripe
                StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];
                var service = new PaymentIntentService();
                var intent = await service.GetAsync(paymentIntentId);

                if (intent.Status != "succeeded")
                {
                    _logger.LogWarning("Payment failed or not confirmed. Status: {Status}", intent.Status);
                    return RedirectToAction(
                        "Failure",
                        new { message = "Le paiement n'a pas été confirmé par le fournisseur de paiement." });
                }

                var total = cart.Items.Sum(i => i.UnitPrice * i.Quantity);

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

                var invoice = new BoutiqueElegance.Models.Invoice
                {
                    OrderId = order.Id,
                    TotalAmount = order.TotalAmount,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Invoices.Add(invoice);

                _context.CartItems.RemoveRange(cart.Items);
                _context.Carts.Remove(cart);
                await _context.SaveChangesAsync();

                return RedirectToAction("Details", "Orders", new { id = order.Id });
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error during payment");
                return RedirectToAction(
                    "Failure",
                    new { message = "Une erreur est survenue avec le paiement Stripe. Veuillez réessayer." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during checkout");
                return RedirectToAction(
                    "Failure",
                    new { message = "Une erreur technique est survenue pendant le traitement de votre commande." });
            }
        }

        [HttpGet]
        public IActionResult Failure(string? message = null)
        {
            ViewBag.ErrorMessage = message ?? "La transaction a échoué. Aucun montant n'a été débité.";
            return View();
        }
    }
}


