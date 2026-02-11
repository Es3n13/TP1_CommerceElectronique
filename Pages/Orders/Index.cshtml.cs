using System.Security.Claims;
using BoutiqueElegance.Data;
using BoutiqueElegance.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BoutiqueElegance.Pages.Orders
{
    [Authorize(Roles = "Client")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Order> Orders { get; set; } = new List<Order>();

        public async Task OnGetAsync()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            Orders = await _context.Orders
                .Where(o => o.ClientId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .Include(o => o.Items)
                .ToListAsync();
        }

        /// Retourne la classe CSS du badge de statut
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

        /// Retourne le label du statut avec emoji
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

