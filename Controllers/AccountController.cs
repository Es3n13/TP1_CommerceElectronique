using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BoutiqueElegance.Data;
using BoutiqueElegance.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BoutiqueElegance.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(ApplicationDbContext context, ILogger<AccountController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError(string.Empty, "Email et mot de passe requis");
                return View();
            }

            try
            {
                var hash = HashPassword(password);
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == email && u.PasswordHash == hash);

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Email ou mot de passe invalide.");
                    return View();
                }

                // Créer les claims et sign in
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role ?? "User")
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties { IsPersistent = true });

                _logger.LogInformation($"User {email} logged in successfully");
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Login error: {ex.Message}");
                ModelState.AddModelError(string.Empty, "Une erreur est survenue lors de la connexion");
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            var restaurants = await _context.Restaurants
                .OrderBy(r => r.Name)
                .ToListAsync();

            ViewBag.Restaurants = restaurants;
            ViewBag.Roles = new List<string> { "Client", "Vendeur" };

            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            string email,
            string password,
            string confirmPassword,
            string fullName,
            string role,
            int? restaurantId)
        {
            if (string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(fullName) ||
                string.IsNullOrEmpty(role))
            {
                ModelState.AddModelError(string.Empty, "Tous les champs sont requis");
            }

            if (role == "Vendeur" && restaurantId == null)
            {
                ModelState.AddModelError(string.Empty, "Vous devez choisir un restaurant pour un compte vendeur");
            }

            if (!ModelState.IsValid)
            {
                // recharger les listes pour la vue
                ViewBag.Restaurants = await _context.Restaurants.OrderBy(r => r.Name).ToListAsync();
                ViewBag.Roles = new List<string> { "Client", "Vendeur" };
                return View();
            }

            if (password != confirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Les mots de passe ne correspondent pas");

                ViewBag.Restaurants = await _context.Restaurants.OrderBy(r => r.Name).ToListAsync();
                ViewBag.Roles = new List<string> { "Client", "Vendeur" };
                return View();
            }

            // Vérifier email existant (comme tu fais déjà)
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (existingUser != null)
            {
                ModelState.AddModelError(string.Empty, "Un compte avec cet email existe déjà");

                ViewBag.Restaurants = await _context.Restaurants.OrderBy(r => r.Name).ToListAsync();
                ViewBag.Roles = new List<string> { "Client", "Vendeur" };
                return View();
            }

            var newUser = new User
            {
                Email = email,
                FullName = fullName,
                PasswordHash = HashPassword(password),
                Role = role
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            if (role == "Vendeur" && restaurantId.HasValue)
            {
                var resto = await _context.Restaurants.FindAsync(restaurantId.Value);
                if (resto != null)
                {
                    resto.SellerId = newUser.Id;
                    _context.Restaurants.Update(resto);
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction("Login");
        }

        // GET: /Account/Profile
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            // Vérifier que l'utilisateur est authentifié
            if (!User.Identity?.IsAuthenticated ?? false)
            {
                return RedirectToAction("Login");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int id))
            {
                return RedirectToAction("Login");
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: /Account/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(int id, string fullName, string email, string phone, string address)
        {
            // Vérifier que l'utilisateur modifie son propre profil
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int currentUserId) || currentUserId != id)
            {
                return Unauthorized();
            }

            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                // Mettre à jour les champs
                user.FullName = fullName ?? user.FullName;
                user.Email = email ?? user.Email;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User {id} profile updated");
                TempData["SuccessMessage"] = "Profil mis à jour avec succès";

                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Profile update error: {ex.Message}");
                ModelState.AddModelError(string.Empty, "Une erreur est survenue");
                return View("Profile");
            }
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _logger.LogInformation("User logged out");
            return RedirectToAction("Index", "Home");
        }

        // Hasher de mot de passe
        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}

