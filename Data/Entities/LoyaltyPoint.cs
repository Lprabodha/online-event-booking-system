using System.ComponentModel.DataAnnotations;

namespace online_event_booking_system.Data.Entities
{
    public class LoyaltyPoint
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string CustomerId { get; set; } = default!;
        public ApplicationUser Customer { get; set; } = default!;
        
        [Range(0, int.MaxValue, ErrorMessage = "Points must be greater than or equal to 0")]
        public int Points { get; set; }
        
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [MaxLength(200)]
        public string? Description { get; set; }
    }
}
