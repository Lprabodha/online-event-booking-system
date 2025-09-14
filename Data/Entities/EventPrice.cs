using System.ComponentModel.DataAnnotations;

namespace online_event_booking_system.Data.Entities
{
    public class EventPrice
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid EventId { get; set; }
        public Event Event { get; set; } = default!;
        
        [Required, MaxLength(100)]
        public string Category { get; set; } = default!;
        
        public int Stock { get; set; }
        public bool IsActive { get; set; } = true;
        
        [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than or equal to 0")]
        public decimal Price { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Additional fields for various price types
        [MaxLength(500)]
        public string? Description { get; set; }
        
        [MaxLength(50)]
        public string PriceType { get; set; } = "Standard"; // Standard, Early Bird, VIP, Group, Student, etc.
        
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidUntil { get; set; }
        public int? MinQuantity { get; set; }
        public int? MaxQuantity { get; set; }

        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
