using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using online_event_booking_system.Data.Entities;

namespace online_event_booking_system.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Event> Events { get; set; }
    public DbSet<Venue> Venues { get; set; }
    public DbSet<EventPrice> EventPrices { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<Payment> Payment { get; set; }
    public DbSet<Discount> Discounts { get; set; }
    public DbSet<LoyaltyPoint> LoyaltyPoints { get; set; }

}
