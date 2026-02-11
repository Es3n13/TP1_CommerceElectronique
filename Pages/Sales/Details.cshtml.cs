using System.Security.Claims;
using BoutiqueElegance.Data;
using BoutiqueElegance.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BoutiqueElegance.Pages.Sales
{
    [Authorize(Roles = "Vendeur")]
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Order? Order { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // Id de l'utilisateur vendeur connecté
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Récupérer le restaurant associé à ce vendeur
            var restaurant = await _context.Restaurants
                .FirstOrDefaultAsync(r => r.SellerId == userId);

            if (restaurant == null)
            {
                return NotFound();
            }

            // Charger la commande uniquement si elle appartient à ce restaurant
            Order = await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Plat)
                .Include(o => o.Invoice)
                .Include(o => o.Restaurant)
                .Include(o => o.Client)
                .FirstOrDefaultAsync(o => o.Id == id && o.RestaurantId == restaurant.Id);

            if (Order == null)
                return NotFound();

            return Page();
        }

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

