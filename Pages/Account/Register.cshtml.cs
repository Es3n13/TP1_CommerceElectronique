using BoutiqueElegance.Data;
using BoutiqueElegance.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace BoutiqueElegance.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public RegisterModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string FullName { get; set; } = string.Empty;

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        public string Role { get; set; } = string.Empty;

        // Pour afficher la liste des restaurants si Role = Vendeur
        public IList<Restaurant> Restaurants { get; set; } = new List<Restaurant>();

        [BindProperty]
        public int? SelectedRestaurantId { get; set; } // seulement pour Vendeur

        public async Task OnGetAsync()
        {
            Restaurants = await _context.Restaurants
                .OrderBy(r => r.Name)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Restaurants = await _context.Restaurants
                .OrderBy(r => r.Name)
                .ToListAsync();

            if (!ModelState.IsValid)
                return Page();

            // Vérifier l'email unique
            if (await _context.Users.AnyAsync(u => u.Email == Email))
            {
                ModelState.AddModelError(string.Empty, "Un compte avec cet email existe déjà.");
                return Page();
            }

            // Si vendeur, un restaurant doit être choisi
            if (Role == "Vendeur" && (SelectedRestaurantId == null || SelectedRestaurantId == 0))
            {
                ModelState.AddModelError(string.Empty, "Veuillez sélectionner un restaurant.");
                return Page();
            }

            var user = new User
            {
                FullName = FullName,
                Email = Email,
                PasswordHash = HashPassword(Password),
                Role = Role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Associer le restaurant au vendeur
            if (Role == "Vendeur" && SelectedRestaurantId.HasValue)
            {
                var restaurant = await _context.Restaurants
                    .FirstOrDefaultAsync(r => r.Id == SelectedRestaurantId.Value);

                if (restaurant != null)
                {
                    restaurant.SellerId = user.Id;
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToPage("/Account/Login");
        }

        private static string HashPassword(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
