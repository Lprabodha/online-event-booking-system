namespace online_event_booking_system.Data.Entities
{
    public class LoyaltyPoint
    {
        public int Id { get; set; }
        public string CustomerId { get; set; } = default!;
        public ApplicationUser Customer { get; set; } = default!;
        public int Points { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
