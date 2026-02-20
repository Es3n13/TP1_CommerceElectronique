using System.Security.Claims;
using BoutiqueElegance.Data;
using BoutiqueElegance.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BoutiqueElegance.Controllers
{
    [Authorize(Roles = "Client")]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var orders = await _context.Orders
                .Where(o => o.ClientId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .Include(o => o.Items)
                .ToListAsync();

            ViewBag.Orders = orders;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var order = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Plat)
                .Include(o => o.Invoice)
                .Include(o => o.Restaurant)
                .FirstOrDefaultAsync(o => o.Id == id && o.ClientId == userId);

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

