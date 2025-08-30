namespace online_event_booking_system.Data.Entities
{
    public class Booking
    {
        public int Id { get; set; }
        public string CustomerId { get; set; }
        public ApplicationUser Customer { get; set; }

        public int EventId { get; set; }
        public Event Event { get; set; }

        public string BookingReference { get; set; } = Guid.NewGuid().ToString("N");
        public string Status { get; set; } = "Confirmed"; // Pending, Confirmed, Cancelled
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
