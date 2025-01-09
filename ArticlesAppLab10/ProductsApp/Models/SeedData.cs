using ProductsApp.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static ProductsApp.Models.ProductCarts;

// PASUL 4: useri si roluri

namespace ProductsApp.Models
{
    public class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
            serviceProvider.GetRequiredService
            <DbContextOptions<ApplicationDbContext>>()))
            {
                // Verificam daca in baza de date exista cel putin un rol
                if (context.Roles.Any())
                {
                    return; // Baza de date contine deja roluri
                }

                // Crearea rolurilor in BD
                context.Roles.AddRange(
                new IdentityRole { Id = "2c5e174e-3b0e-446f-86af-483d56fd7210", Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole { Id = "2c5e174e-3b0e-446f-86af-483d56fd7211", Name = "Colaborator", NormalizedName = "COLABORATOR" },
                new IdentityRole { Id = "2c5e174e-3b0e-446f-86af-483d56fd7212", Name = "User", NormalizedName = "USER" }
                );

                // Instanta pentru crearea parolelor utilizatorilor
                var hasher = new PasswordHasher<ApplicationUser>();

                // Crearea utilizatorilor in BD
                context.Users.AddRange(
                new ApplicationUser
                {
                    Id = "8e445865-a24d-4543-a6c6-9443d048cdb0",
                    UserName = "admin@test.com",
                    EmailConfirmed = true,
                    NormalizedEmail = "ADMIN@TEST.COM",
                    Email = "admin@test.com",
                    NormalizedUserName = "ADMIN@TEST.COM",
                    PasswordHash = hasher.HashPassword(null, "Admin1!")
                },

                new ApplicationUser
                {
                    Id = "8e445865-a24d-4543-a6c6-9443d048cdb1",
                    UserName = "colaborator@test.com",
                    EmailConfirmed = true,
                    NormalizedEmail = "COLABORATOR@TEST.COM",
                    Email = "colaborator@test.com",
                    NormalizedUserName = "COLABORATOR@TEST.COM",
                    PasswordHash = hasher.HashPassword(null, "Colaborator1!")
                },

                new ApplicationUser
                {
                    Id = "8e445865-a24d-4543-a6c6-9443d048cdb2",
                    UserName = "user@test.com",
                    EmailConfirmed = true,
                    NormalizedEmail = "USER@TEST.COM",
                    Email = "user@test.com",
                    NormalizedUserName = "USER@TEST.COM",
                    PasswordHash = hasher.HashPassword(null, "User1!")
                }
                );

                // Asocierea user-urilor la roluri
                context.UserRoles.AddRange(
                new IdentityUserRole<string>
                {
                    RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7210",
                    UserId = "8e445865-a24d-4543-a6c6-9443d048cdb0"
                },
                new IdentityUserRole<string>
                {
                    RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7211",
                    UserId = "8e445865-a24d-4543-a6c6-9443d048cdb1"
                },
                new IdentityUserRole<string>
                {
                    RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7212",
                    UserId = "8e445865-a24d-4543-a6c6-9443d048cdb2"
                }
                );

                foreach (var user in context.Users)
                {
                    if (!context.Carts.Any(c => c.UserId == user.Id))
                    {
                        context.Carts.Add(new Cart
                        {
                            UserId = user.Id,
                            ProductCarts = new List<ProductCart>()
                        });
                    }
                }

                context.SaveChanges();
            }
        }
    }
}
