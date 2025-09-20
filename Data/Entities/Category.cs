using System.ComponentModel.DataAnnotations;

namespace online_event_booking_system.Data.Entities
{
    /// <summary>
    /// Represents a category for events in the online event booking system.
    /// </summary>
    public class Category
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required, MaxLength(100)]
        public string Name { get; set; } = default!;
        
        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(10)]
        public string? Icon { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
}
