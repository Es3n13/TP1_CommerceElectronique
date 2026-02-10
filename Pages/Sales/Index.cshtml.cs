using System.Security.Claims;
using BoutiqueElegance.Data;
using BoutiqueElegance.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BoutiqueElegance.Pages.Sales
{
    [Authorize(Roles = "Vendeur")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Restaurant? Restaurant { get; set; }
        public IList<Order> Orders { get; set; } = new List<Order>();
        public decimal TotalRestaurantRevenue => Orders.Sum(o => o.TotalAmount);

        public async Task OnGetAsync()
        {
            var sellerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Récupérer le restaurant de ce vendeur
            Restaurant = await _context.Restaurants
                .FirstOrDefaultAsync(r => r.SellerId == sellerId);

            if (Restaurant == null)
            {
                // Pas encore de restaurant associé
                Orders = new List<Order>();
                return;
            }

            Orders = await _context.Orders
                .Include(o => o.Client)
                .Where(o => o.RestaurantId == Restaurant.Id && o.Status == "Paid")
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }
    }
}

