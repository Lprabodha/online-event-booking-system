using Microsoft.EntityFrameworkCore;
using online_event_booking_system.Data.Entities;

namespace online_event_booking_system.Data.Seeders
{
    public class CategorySeeder
    {
        public static async Task SeedCategoriesAsync(ApplicationDbContext context)
        {
            if (await context.Categories.AnyAsync())
            {
                return;
            }

            var categories = new List<Category>
        {
            new Category { Name = "Music", Description = "Concerts and music performances", Icon = "🎵" },
            new Category { Name = "Theatre", Description = "Plays, musicals, and stage performances", Icon = "🎭" },
            new Category { Name = "Sport", Description = "Sporting events and games", Icon = "⚽" },
            new Category { Name = "Festival", Description = "Multi-day events, including music, arts, and food", Icon = "🎪" },
            new Category { Name = "Conference", Description = "Professional and academic gatherings", Icon = "🗣️" },
            new Category { Name = "Art & Exhibit", Description = "Art shows, gallery openings, and museum exhibits", Icon = "🎨" },
            new Category { Name = "Comedy", Description = "Stand-up comedy and improv shows", Icon = "😂" },
            new Category { Name = "Food & Drink", Description = "Food festivals, cooking classes, and wine tastings", Icon = "🍷" },
            new Category { Name = "Family", Description = "Events suitable for all ages, including kids' shows and activities", Icon = "👨‍👩‍👧‍👦" }
        };

            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }
    }
}
