using System.ComponentModel.DataAnnotations;

namespace online_event_booking_system.Data.Entities
{
    /// <summary>
    /// Represents a discount or promotional code that can be applied to event bookings.
    /// </summary>
    public class Discount
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid? EventId { get; set; }
        public Event? Event { get; set; }
        
        [Required, MaxLength(50)]
        public string Code { get; set; } = default!;
        
        [Required, MaxLength(20)]
        public string Type { get; set; } = "Percent"; // Percent, Amount
        
        [Range(0, double.MaxValue, ErrorMessage = "Value must be greater than or equal to 0")]
        public decimal Value { get; set; }
        
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public bool IsActive { get; set; } = true;
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        public int? UsageLimit { get; set; }
        public int UsedCount { get; set; } = 0;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
