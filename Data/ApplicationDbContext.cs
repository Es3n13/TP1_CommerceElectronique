using BoutiqueElegance.Models;
using Microsoft.EntityFrameworkCore;

namespace BoutiqueElegance.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Restaurant> Restaurants { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<RestaurantTag> RestaurantTags { get; set; }
        public DbSet<Plat> Plats { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Invoice> Invoices { get; set; }

        public static class DbInitializer
        {
            public static void Initialize(ApplicationDbContext context)
            {
                context.Database.Migrate();

                // Si des restaurants existent déjà, on ne seed pas deux fois
                if (context.Restaurants.Any())
                    return;

                // Créer les tags
                string[] tagNames = { "Togolaise", "Italienne", "Française", "Asiatique" };

                foreach (var name in tagNames)
                {
                    if (!context.Tags.Any(t => t.Name == name))
                    {
                        context.Tags.Add(new Tag { Name = name });
                    }
                }
                context.SaveChanges();

                // Récupérer les tags
                var togolaise = context.Tags.Single(t => t.Name == "Togolaise");
                var italienne = context.Tags.Single(t => t.Name == "Italienne");
                var francaise = context.Tags.Single(t => t.Name == "Française");

                // Créer les restaurants
                var r1 = new Restaurant
                {
                    Name = "Mama Togo",
                    Description = "Cuisine togolaise authentique à emporter.",
                    Address = "123 Rue du Togo, Rimouski",
                    ImageUrl = "https://via.placeholder.com/400x250"
                };

                var r2 = new Restaurant
                {
                    Name = "Trattoria Roma",
                    Description = "Pâtes fraîches et pizzas au feu de bois.",
                    Address = "45 Avenue d'Italie, Rimouski",
                    ImageUrl = "https://via.placeholder.com/400x250"
                };

                var r3 = new Restaurant
                {
                    Name = "Bistro Paris",
                    Description = "Classiques français revisités.",
                    Address = "78 Rue de Paris, Rimouski",
                    ImageUrl = "https://via.placeholder.com/400x250"
                };

                context.Restaurants.AddRange(r1, r2, r3);
                context.SaveChanges();

                // Lier restaurants et tags (RestaurantTags)
                context.RestaurantTags.AddRange(
                    new RestaurantTag { RestaurantId = r1.Id, TagId = togolaise.Id },
                    new RestaurantTag { RestaurantId = r2.Id, TagId = italienne.Id },
                    new RestaurantTag { RestaurantId = r3.Id, TagId = francaise.Id }
                );
                context.SaveChanges();

                // Plats
                var plats = new[]
                {
                    new Plat {
                        Name = "Riz gras au poulet",
                        Description = "Plat emblématique togolais.",
                        Price = 14.99m,
                        ImageUrl = "https://via.placeholder.com/400x250",
                        Category = "Plat principal",
                        RestaurantId = r1.Id
                    },
                    new Plat {
                        Name = "Pizza Margherita",
                        Description = "Tomate, mozzarella, basilic.",
                        Price = 16.50m,
                        ImageUrl = "https://via.placeholder.com/400x250",
                        Category = "Plat principal",
                        RestaurantId = r2.Id
                    },
                    new Plat {
                        Name = "Crème brûlée",
                        Description = "Dessert français classique.",
                        Price = 8.25m,
                        ImageUrl = "https://via.placeholder.com/400x250",
                        Category = "Dessert",
                        RestaurantId = r3.Id
                    }
                };

                context.Plats.AddRange(plats);
                context.SaveChanges();
            }
        }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Many‑to‑many Restaurant–Tag
            modelBuilder.Entity<RestaurantTag>()
                .HasKey(rt => new { rt.RestaurantId, rt.TagId });

            modelBuilder.Entity<RestaurantTag>()
                .HasOne(rt => rt.Restaurant)
                .WithMany(r => r.RestaurantTags)
                .HasForeignKey(rt => rt.RestaurantId);

            modelBuilder.Entity<RestaurantTag>()
                .HasOne(rt => rt.Tag)
                .WithMany(t => t.RestaurantTags)
                .HasForeignKey(rt => rt.TagId);

            // Unicité du nom de tag
            modelBuilder.Entity<Tag>()
                .HasIndex(t => t.Name)
                .IsUnique();

            //Many-to-one Order-Restaurant
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Restaurant)
                .WithMany()
                .HasForeignKey(o => o.RestaurantId)
                .OnDelete(DeleteBehavior.Restrict);

            //Restaurant-Seller
            modelBuilder.Entity<Restaurant>()
                .HasOne(r => r.Seller)
                .WithMany()
                .HasForeignKey(r => r.SellerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
