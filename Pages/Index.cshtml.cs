using BoutiqueElegance.Data;
using BoutiqueElegance.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BoutiqueElegance.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // Liste des restaurants
        public IList<Restaurant> Restaurants { get; set; } = new List<Restaurant>();

        // Tous les tags
        public IList<Tag> AllTags { get; set; } = new List<Tag>();

        // Filtre
        [BindProperty(SupportsGet = true)]
        public string? SelectedTag { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        public async Task OnGetAsync()
        {
            // Charger les tags pour les boutons
            AllTags = await _context.Tags
                .OrderBy(t => t.Name)
                .ToListAsync();

            // Query : restaurants + leurs tags
            var query = _context.Restaurants
                .Include(r => r.RestaurantTags)
                    .ThenInclude(rt => rt.Tag)
                .AsQueryable();

            // Filtre par tag
            if (!string.IsNullOrEmpty(SelectedTag))
            {
                query = query.Where(r =>
                    r.RestaurantTags.Any(rt => rt.Tag.Name == SelectedTag));
            }

            // Filtre par recherche nom
            if (!string.IsNullOrWhiteSpace(Search))
            {
                query = query.Where(r =>
                    r.Name.Contains(Search));
            }

            Restaurants = await query.ToListAsync();
        }
    }
}
