using BoutiqueElegance.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BoutiqueElegance.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search, string? selectedTag)
        {
            // Charger tous les tags pour le dropdown de filtre
            ViewBag.AllTags = await _context.Tags
                .OrderBy(t => t.Name)
                .ToListAsync();

            // Requête : restaurants + leurs tags
            var query = _context.Restaurants
                .Include(r => r.RestaurantTags)
                .ThenInclude(rt => rt.Tag)
                .AsQueryable();

            // Filtre par tag si fourni
            if (!string.IsNullOrEmpty(selectedTag))
            {
                query = query.Where(r =>
                    r.RestaurantTags.Any(rt => rt.Tag.Name == selectedTag));
            }

            // Filtre par texte de recherche si fourni
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(r =>
                    r.Name.Contains(search));
            }

            // Charger les restaurants filtrés
            ViewBag.Restaurants = await query.ToListAsync();
            ViewBag.SelectedTag = selectedTag;
            ViewBag.Search = search;

            return View();
        }
    }
}

