using System.ComponentModel.DataAnnotations;

namespace online_event_booking_system.Data.Entities
{
    public class Event
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid VenueId { get; set; }
        public Venue Venue { get; set; } = default!;
        public string OrganizerId { get; set; } = default!;
        public ApplicationUser Organizer { get; set; } = default!;
        public Guid CategoryId { get; set; }
        public Category Category { get; set; } = default!;

        [Required, MaxLength(200)]
        public string Title { get; set; } = default!;
        public string Description { get; set; } = default!;
        public DateTime EventDate { get; set; }
        public DateTime EventTime { get; set; }
        public string? Image { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
        public ICollection<EventPrice> Prices { get; set; } = new List<EventPrice>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Discount> Discounts { get; set; } = new List<Discount>();
    }
}
