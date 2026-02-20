using BoutiqueElegance.Data;
using BoutiqueElegance.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BoutiqueElegance.Controllers
{
    public class RestaurantsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RestaurantsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var restaurant = await _context.Restaurants
                .Include(r => r.RestaurantTags)
                .ThenInclude(rt => rt.Tag)
                .Include(r => r.Plats)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (restaurant == null)
                return NotFound();

            ViewBag.Restaurant = restaurant;
            return View();
        }

        /// Formate les tags/cuisine du restaurant
        public string GetCuisineDisplay(Restaurant restaurant)
        {
            if (restaurant?.RestaurantTags == null || !restaurant.RestaurantTags.Any())
                return "Cuisine générale";
            return string.Join(", ", restaurant.RestaurantTags.Select(rt => rt.Tag.Name));
        }
    }
}
