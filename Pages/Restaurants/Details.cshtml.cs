using BoutiqueElegance.Data;
using BoutiqueElegance.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BoutiqueElegance.Pages.Restaurants
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Restaurant? Restaurant { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Restaurant = await _context.Restaurants
                .Include(r => r.RestaurantTags)
                    .ThenInclude(rt => rt.Tag)
                .Include(r => r.Plats)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (Restaurant == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}
