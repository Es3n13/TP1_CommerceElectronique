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

                // Si des restaurants existent déjà, on seed pas 2x
                if (context.Restaurants.Any())
                    return;

                // Créer les tags
                string[] tagNames = { "Africaine", "Italienne", "Française", "Asiatique" };

                foreach (var name in tagNames)
                {
                    if (!context.Tags.Any(t => t.Name == name))
                    {
                        context.Tags.Add(new Tag { Name = name });
                    }
                }
                context.SaveChanges();

                // Récupérer les tags
                var africaine = context.Tags.Single(t => t.Name == "Africiane");
                var italienne = context.Tags.Single(t => t.Name == "Italienne");
                var francaise = context.Tags.Single(t => t.Name == "Française");

                // Créer les restaurants
                var r1 = new Restaurant
                {
                    Name = "Mama Togo",
                    Description = "Cuisine ouest africaine authentique.",
                    Address = "123 Rue du Togo, Rimouski",
                    CardImageUrl = "/images/restaurants/mamaTogo.png",
                    BannerImageUrl = "/images/restaurants/mamaTogo_Banner.png"
                };

                var r2 = new Restaurant
                {
                    Name = "Trattoria Roma",
                    Description = "Pâtes fraîches et pizzas au feu de bois.",
                    Address = "45 Avenue d'Italie, Rimouski",
                    CardImageUrl = "/images/restaurants/restaurant2.png",
                    BannerImageUrl = "/images/restaurants/Resto2_Banner.png"
                };

                var r3 = new Restaurant
                {
                    Name = "Bistro Lomé",
                    Description = "Classiques africains et français revisités.",
                    Address = "78 Rue de Paris, Rimouski",
                    CardImageUrl = "/images/restaurants/restaurant3.png",
                    BannerImageUrl = "/images/restaurants/Resto3_Banner.png"
                }; ;

                context.Restaurants.AddRange(r1, r2, r3);
                context.SaveChanges();

                // Lier restaurants et tags (RestaurantTags)
                context.RestaurantTags.AddRange(
                    new RestaurantTag { RestaurantId = r1.Id, TagId = africaine.Id },
                    new RestaurantTag { RestaurantId = r2.Id, TagId = italienne.Id },
                    new RestaurantTag { RestaurantId = r3.Id, TagId = francaise.Id }
                );
                context.SaveChanges();

                // Plats
                var plats = new[]
                {
                    //Mama Togo
                    new Plat {
                        Name = "Pastels au bœuf",
                        Description = "Chaussons frits farcis de bœuf épicé, servis avec une sauce pimentée douce.",
                        Price = 9.50m,
                        ImageUrl = "/images/plats/pastels-boeuf.jpg",
                        Category = "Entrée",
                        RestaurantId = r1.Id
                    },
                    new Plat {
                        Name = "Akpan & sauce piquante",
                        Description = "Pâte de maïs fermenté légèrement acidulée avec sauce tomate pimentée et oignons frits.",
                        Price = 8.90m,
                        ImageUrl = "/images/plats/akpan.jpg",
                        Category = "Entrée",
                        RestaurantId = r1.Id
                    },
                    new Plat {
                        Name = "Brochettes de gésiers",
                        Description = "Gésiers marinés aux épices togolaises, grillés et servis sur brochettes.",
                        Price = 10.50m,
                        ImageUrl = "/images/plats/gesiers-brochettes.jpg",
                        Category = "Entrée",
                        RestaurantId = r1.Id
                    },

                    new Plat {
                        Name = "Riz gras au poulet",
                        Description = "Riz parfumé cuit dans une sauce tomate épicée, accompagné de poulet rôti et de légumes.",
                        Price = 14.99m,
                        ImageUrl = "/images/plats/riz-gras-poulet.jpg",
                        Category = "Plat principal",
                        RestaurantId = r1.Id
                    },
                    new Plat {
                        Name = "Poulet Yassa à la togolaise",
                        Description = "Poulet mariné au citron, oignons et moutarde, servi avec riz blanc ou attiéké.",
                        Price = 18.90m,
                        ImageUrl = "/images/plats/poulet-yassa.jpg",
                        Category = "Plat principal",
                        RestaurantId = r1.Id
                    },
                    new Plat {
                        Name = "Sauce arachide & agneau",
                        Description = "Ragoût d’agneau tendre dans une sauce épaisse à la pâte d’arachide.",
                        Price = 19.90m,
                        ImageUrl = "/images/plats/agneau-arachide.jpg",
                        Category = "Plat principal",
                        RestaurantId = r1.Id
                    },
                    new Plat {
                        Name = "Foufou & sauce graine",
                        Description = "Foufou de manioc et plantain servi avec sauce graine, poisson fumé et légumes.",
                        Price = 19.50m,
                        ImageUrl = "/images/plats/foufou-sauce-graine.jpg",
                        Category = "Plat principal",
                        RestaurantId = r1.Id
                    },
                    new Plat {
                        Name = "Poisson braisé à la togolaise",
                        Description = "Poisson entier mariné au citron, ail et piment, grillé et servi avec alloco.",
                        Price = 22.00m,
                        ImageUrl = "/images/plats/poisson-braise.jpg",
                        Category = "Plat principal",
                        RestaurantId = r1.Id
                    },

                    new Plat {
                        Name = "Alloco croustillant",
                        Description = "Bananes plantains frites et caramélisées, servies avec sauce pimentée maison.",
                        Price = 7.90m,
                        ImageUrl = "/images/plats/alloco.jpg",
                        Category = "Accompagnement",
                        RestaurantId = r1.Id
                    },
                    new Plat {
                        Name = "Attiéké au beurre d’ail",
                        Description = "Semoule de manioc légère, parfumée au beurre d’ail et aux herbes.",
                        Price = 6.90m,
                        ImageUrl = "/images/plats/attieke.jpg",
                        Category = "Accompagnement",
                        RestaurantId = r1.Id
                    },

                    new Plat {
                        Name = "Beignets de banane plantain",
                        Description = "Beignets moelleux à base de banane plantain, coulis de mangue.",
                        Price = 8.25m,
                        ImageUrl = "/images/plats/beignets-banane.jpg",
                        Category = "Dessert",
                        RestaurantId = r1.Id
                    },
                    new Plat {
                        Name = "Gâteau de manioc coco",
                        Description = "Gâteau fondant de manioc râpé au lait de coco, parfumé à la vanille.",
                        Price = 8.90m,
                        ImageUrl = "/images/plats/gateau-manioc-coco.jpg",
                        Category = "Dessert",
                        RestaurantId = r1.Id
                    },

                    //TRattoria Roma
                    new Plat {
                        Name = "Bruschetta fusion",
                        Description = "Pain grillé, tomates confites, huile d’olive et touche d’huile pimentée africaine.",
                        Price = 11.50m,
                        ImageUrl = "/images/plats/bruschetta-fusion.jpg",
                        Category = "Entrée",
                        RestaurantId = r2.Id
                    },
                    new Plat {
                        Name = "Arancini maïs & fromage de brebis",
                        Description = "Boulettes de riz croustillantes, cœur fondant de maïs et fromage de brebis.",
                        Price = 12.90m,
                        ImageUrl = "/images/plats/arancini-mais.jpg",
                        Category = "Entrée",
                        RestaurantId = r2.Id
                    },
                    new Plat {
                        Name = "Carpaccio de bœuf aux épices africaines",
                        Description = "Fines tranches de bœuf, marinade citronnée, mélange d’épices africaines, copeaux de parmesan.",
                        Price = 15.50m,
                        ImageUrl = "/images/plats/carpaccio-africain.jpg",
                        Category = "Entrée",
                        RestaurantId = r2.Id
                    },

                    new Plat {
                        Name = "Tagliatelles sauce arachide & poulet grillé",
                        Description = "Pâtes fraîches, sauce crémeuse à la cacahuète, éclats de poulet grillé et herbes fraîches.",
                        Price = 22.90m,
                        ImageUrl = "/images/plats/tagliatelles-arachide.jpg",
                        Category = "Plat principal",
                        RestaurantId = r2.Id
                    },
                    new Plat {
                        Name = "Lasagne à la merguez et légumes du soleil",
                        Description = "Lasagne gratinée, merguez épicée, poivrons, courgettes et tomate confite.",
                        Price = 23.50m,
                        ImageUrl = "/images/plats/lasagne-merguez.jpg",
                        Category = "Plat principal",
                        RestaurantId = r2.Id
                    },
                    new Plat {
                        Name = "Gnocchis au ragoût d’agneau façon mafé",
                        Description = "Gnocchis maison nappés d’un ragoût d’agneau à la sauce arachide.",
                        Price = 24.90m,
                        ImageUrl = "/images/plats/gnocchis-mafe.jpg",
                        Category = "Plat principal",
                        RestaurantId = r2.Id
                    },
                    new Plat {
                        Name = "Dorade rôtie, polenta crémeuse & chermoula",
                        Description = "Filet de dorade rôtie, polenta au parmesan, sauce chermoula citronnée.",
                        Price = 26.00m,
                        ImageUrl = "/images/plats/dorade-polenta.jpg",
                        Category = "Plat principal",
                        RestaurantId = r2.Id
                    },

                    new Plat {
                        Name = "Tiramisu à la fleur d’oranger",
                        Description = "Tiramisu léger parfumé à la fleur d’oranger et café.",
                        Price = 11.00m,
                        ImageUrl = "/images/plats/tiramisu-fleur-oranger.jpg",
                        Category = "Dessert",
                        RestaurantId = r2.Id
                    },
                    new Plat {
                        Name = "Panna cotta mangue & hibiscus",
                        Description = "Panna cotta vanille, coulis de mangue et gel d’hibiscus.",
                        Price = 10.50m,
                        ImageUrl = "/images/plats/pannacotta-mangue-hibiscus.jpg",
                        Category = "Dessert",
                        RestaurantId = r2.Id
                    },

                    // Bristro Lomé
                    new Plat {
                        Name = "Filet de Bœuf Wagyu",
                        Description = "Pièce d’exception grillée, purée truffée, réduction au vin rouge, légumes glacés de saison.",
                        Price = 48.00m,
                        ImageUrl = "/images/plats/filet-wagyu.jpg",
                        Category = "Plat principal",
                        RestaurantId = r3.Id
                    },
                    new Plat {
                        Name = "Saumon Royal Label Rouge",
                        Description = "Pavé croustillant, risotto crémeux au citron, émulsion d’herbes fraîches.",
                        Price = 34.00m,
                        ImageUrl = "/images/plats/saumon-royal.jpg",
                        Category = "Plat principal",
                        RestaurantId = r3.Id
                    },
                    new Plat {
                        Name = "Risotto aux Morilles",
                        Description = "Riz arborio mijoté, morilles fraîches, parmesan affiné 24 mois, huile d’olive extra vierge.",
                        Price = 29.00m,
                        ImageUrl = "/images/plats/risotto-morilles.jpg",
                        Category = "Plat principal",
                        RestaurantId = r3.Id
                    },
                    new Plat {
                        Name = "Magret de Canard Rossini",
                        Description = "Magret rôti, foie gras poêlé, sauce miel balsamique, gratin dauphinois.",
                        Price = 38.00m,
                        ImageUrl = "/images/plats/magret-rossini.jpg",
                        Category = "Plat principal",
                        RestaurantId = r3.Id
                    },
                    new Plat {
                        Name = "Dôme Chocolat Grand Cru",
                        Description = "Dôme au chocolat noir, cœur coulant, glace vanille bourbon, éclats d’or alimentaire.",
                        Price = 16.00m,
                        ImageUrl = "/images/plats/dome-chocolat.jpg",
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

            //Nom de tag unique
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
