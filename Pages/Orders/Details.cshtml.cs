using System.Security.Claims;
using BoutiqueElegance.Data;
using BoutiqueElegance.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BoutiqueElegance.Pages.Orders
{
    [Authorize(Roles = "Client")]
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
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            Order = await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Plat)
                .Include(o => o.Invoice)
                .FirstOrDefaultAsync(o => o.Id == id && o.ClientId == userId);

            if (Order == null)
                return NotFound();

            return Page();
        }
    }
}

