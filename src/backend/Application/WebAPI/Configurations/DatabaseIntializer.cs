using Data.CommonConstants;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using RedBook.Core.Constants;
using System.Text;

namespace WebAPI.Configurations
{
    /// <summary>
    /// This class creates database if it doesn't exist and initializes database with seed data
    /// </summary>
    public static class DatabaseIntializer
    {
        /// <summary>
        /// Creates database if it doesn't exist and initializes database with seed data
        /// </summary>
        public static void InitDatabase(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                AppDBContext context = scope.ServiceProvider.GetRequiredService<AppDBContext>();

                // Only run database migrations in development environment
                if (env.IsDevelopment())
                {
                    if (context.Database.GetPendingMigrations().Any())
                    {
                        context.Database.Migrate();
                    }

                    SeedData(context);
                }
            }
        }

        private static void SeedData(AppDBContext context)
        {
            context.Database.EnsureCreated();

            #region Create SysAdmin User
            User? sysAdminUser = context.Users.AsTracking().FirstOrDefault(x =>
                x.FirstName == "Md. Nafis"
                && x.LastName == "Sadik"
                && x.UserName == "nafis_sadik"
            );

            if (sysAdminUser == null)
            {
                sysAdminUser = context.Users.Add(new User
                {
                    FirstName = "Md. Nafis",
                    LastName = "Sadik",
                    UserName = "nafis_sadik",
                    Password = BCrypt.Net.BCrypt.HashPassword("a8i#YJVFzk9N", CommonConstants.SecurityConfig.SaltStr),
                    Status = true,
                    Email = "nafis.sadik13@yahoo.com",
                    Address = "12072 Betsy Manors, Tuanstad, ND 16008-8307",
                    PhoneNumber = "(888) 900 1994",
                    Role = CommonAttributeConstants.Roles.Admin
                }).Entity;

                context.SaveChanges();
            }
            else
            {
                sysAdminUser.Email = "nafis.sadik13@yahoo.com";
                sysAdminUser.Password = BCrypt.Net.BCrypt.HashPassword("a8i#YJVFzk9N", CommonConstants.SecurityConfig.SaltStr);
                sysAdminUser.Status = true;
                sysAdminUser.Role = CommonAttributeConstants.Roles.Admin;

                context.SaveChanges();
            }
            #endregion

            context.SaveChanges();

            context.Dispose();
        }
    }
}
