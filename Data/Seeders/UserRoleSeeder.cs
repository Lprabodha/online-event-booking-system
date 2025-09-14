using Microsoft.AspNetCore.Identity;
using online_event_booking_system.Data.Entities;

namespace online_event_booking_system.Data.Seeders
{
    public class UserRoleSeeder
    {
        public static async Task SeedRoles(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            string[] roles = { "Admin", "Organizer", "Customer" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        public static async Task SeedInitialUsers(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roles = { "Admin", "Organizer", "Customer" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Define users per role
            var usersToSeed = new List<(string email, string role, string fullName)>
            {
                ("admin@example.com",    "Admin",    "System Admin"),
                ("organizer@example.com","Organizer","Demo Organizer"),
                ("customer@example.com", "Customer", "Demo Customer"),
            };

            foreach (var (email, role, fullName) in usersToSeed)
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    var newUser = new ApplicationUser
                    {
                        FullName = fullName,
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true,
                    };

                    var result = await userManager.CreateAsync(newUser, "Password@123");

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(newUser, role);
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            Console.WriteLine($"{email} - Error: {error.Description}");
                        }
                    }
                }
            }
        }
    }
}
