using System.Security.Claims;
using BoutiqueElegance.Data;
using BoutiqueElegance.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BoutiqueElegance.Controllers
{
    [Authorize(Roles = "Vendeur")]
    public class SalesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SalesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var sellerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Récupérer le restaurant de ce vendeur
            var restaurant = await _context.Restaurants
                .FirstOrDefaultAsync(r => r.SellerId == sellerId);

            if (restaurant == null)
            {
                // Pas encore de restaurant associé
                ViewBag.Restaurant = null;
                ViewBag.Orders = new List<Order>();
                ViewBag.TotalRestaurantRevenue = 0m;
                return View();
            }

            var orders = await _context.Orders
                .Include(o => o.Client)
                .Where(o => o.RestaurantId == restaurant.Id && o.Status == "Paid")
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            ViewBag.Restaurant = restaurant;
            ViewBag.Orders = orders;
            ViewBag.TotalRestaurantRevenue = orders.Sum(o => o.TotalAmount);

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            // Id de l'utilisateur vendeur connecté
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Récupérer le restaurant associé à ce vendeur
            var restaurant = await _context.Restaurants
                .FirstOrDefaultAsync(r => r.SellerId == userId);

            if (restaurant == null)
                return NotFound();

            // Charger la commande uniquement si elle appartient à ce restaurant
            var order = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Plat)
                .Include(o => o.Invoice)
                .Include(o => o.Restaurant)
                .Include(o => o.Client)
                .FirstOrDefaultAsync(o => o.Id == id && o.RestaurantId == restaurant.Id);

            if (order == null)
                return NotFound();

            ViewBag.Order = order;
            return View();
        }

        // Retourne la classe CSS du badge de statut
        public string GetStatusClass(string status)
        {
            return status?.ToLower() switch
            {
                "pending" => "status-pending",
                "paid" => "status-paid",
                "failed" => "status-failed",
                _ => "status-pending"
            };
        }

        // Retourne le label du statut en français
        public string GetStatusLabel(string status)
        {
            return status?.ToLower() switch
            {
                "pending" => "En attente",
                "paid" => "Payée",
                "failed" => "Échouée",
                _ => status ?? "Inconnue"
            };
        }
    }
}

