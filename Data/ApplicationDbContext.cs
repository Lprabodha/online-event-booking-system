using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using online_event_booking_system.Data.Entities;

namespace online_event_booking_system.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Event> Events { get; set; }
    public DbSet<Venue> Venues { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<EventPrice> EventPrices { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Discount> Discounts { get; set; }
    public DbSet<LoyaltyPoint> LoyaltyPoints { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure Event relationships
        builder.Entity<Event>()
            .HasOne(e => e.Venue)
            .WithMany(v => v.Events)
            .HasForeignKey(e => e.VenueId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Event>()
            .HasOne(e => e.Organizer)
            .WithMany()
            .HasForeignKey(e => e.OrganizerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Event>()
            .HasOne(e => e.Category)
            .WithMany(c => c.Events)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Booking relationships
        builder.Entity<Booking>()
            .HasOne(b => b.Customer)
            .WithMany()
            .HasForeignKey(b => b.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Booking>()
            .HasOne(b => b.Event)
            .WithMany(e => e.Bookings)
            .HasForeignKey(b => b.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Ticket relationships
        builder.Entity<Ticket>()
            .HasOne(t => t.Customer)
            .WithMany(u => u.Tickets)
            .HasForeignKey(t => t.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Ticket>()
            .HasOne(t => t.Booking)
            .WithMany(b => b.Tickets)
            .HasForeignKey(t => t.BookingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Ticket>()
            .HasOne(t => t.Event)
            .WithMany(e => e.Tickets)
            .HasForeignKey(t => t.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Ticket>()
            .HasOne(t => t.EventPrice)
            .WithMany(ep => ep.Tickets)
            .HasForeignKey(t => t.EventPriceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Ticket>()
            .HasOne(t => t.Payment)
            .WithMany(p => p.Tickets)
            .HasForeignKey(t => t.PaymentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure EventPrice relationships
        builder.Entity<EventPrice>()
            .HasOne(ep => ep.Event)
            .WithMany(e => e.Prices)
            .HasForeignKey(ep => ep.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure Payment relationships
        builder.Entity<Payment>()
            .HasOne(p => p.Customer)
            .WithMany(u => u.Payments)
            .HasForeignKey(p => p.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Discount relationships
        builder.Entity<Discount>()
            .HasOne(d => d.Event)
            .WithMany(e => e.Discounts)
            .HasForeignKey(d => d.EventId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure LoyaltyPoint relationships
        builder.Entity<LoyaltyPoint>()
            .HasOne(lp => lp.Customer)
            .WithMany()
            .HasForeignKey(lp => lp.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure unique constraints
        builder.Entity<Booking>()
            .HasIndex(b => b.BookingReference)
            .IsUnique();

        builder.Entity<Ticket>()
            .HasIndex(t => t.TicketNumber)
            .IsUnique();

        builder.Entity<Discount>()
            .HasIndex(d => d.Code)
            .IsUnique();

        // Configure decimal precision
        builder.Entity<EventPrice>()
            .Property(ep => ep.Price)
            .HasPrecision(18, 2);

        builder.Entity<Payment>()
            .Property(p => p.Amount)
            .HasPrecision(18, 2);

        builder.Entity<Payment>()
            .Property(p => p.DiscountAmount)
            .HasPrecision(18, 2);

        builder.Entity<Discount>()
            .Property(d => d.Value)
            .HasPrecision(18, 2);
    }
}
