using System.ComponentModel.DataAnnotations;

namespace online_event_booking_system.Data.Entities
{
    public class Booking
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string CustomerId { get; set; } = default!;
        public ApplicationUser Customer { get; set; } = default!;

        public Guid EventId { get; set; }
        public Event Event { get; set; } = default!;

        [Required, MaxLength(50)]
        public string BookingReference { get; set; } = Guid.NewGuid().ToString("N");
        
        [Required, MaxLength(20)]
        public string Status { get; set; } = "Confirmed"; // Pending, Confirmed, Cancelled
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
