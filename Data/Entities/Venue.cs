using System.ComponentModel.DataAnnotations;

namespace online_event_booking_system.Data.Entities
{
    public class Venue
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required(ErrorMessage = "Venue name is required")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Venue name must be between 2 and 200 characters")]
        [Display(Name = "Venue Name")]
        public string Name { get; set; } = default!;
        
        [Required(ErrorMessage = "Location is required")]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "Location must be between 5 and 500 characters")]
        [Display(Name = "Location")]
        public string Location { get; set; } = default!;
        
        [Required(ErrorMessage = "Capacity is required")]
        [Range(1, 100000, ErrorMessage = "Capacity must be between 1 and 100,000")]
        [Display(Name = "Capacity")]
        public int Capacity { get; set; }
        
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }
        
        [Url(ErrorMessage = "Please enter a valid URL for the image")]
        [Display(Name = "Image URL")]
        public string? Image { get; set; }
        
        [StringLength(500, ErrorMessage = "Contact information cannot exceed 500 characters")]
        [Display(Name = "Contact Information")]
        public string? ContactInfo { get; set; }
        
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
}
