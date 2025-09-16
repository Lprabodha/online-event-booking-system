using System.ComponentModel.DataAnnotations;
using online_event_booking_system.Data.Entities;

namespace online_event_booking_system.Models.View_Models
{
    public class CreateEventViewModel
    {
        [Required(ErrorMessage = "Event title is required")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Event description is required")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 2000 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required")]
        public Guid CategoryId { get; set; }

        [Required(ErrorMessage = "Venue is required")]
        public Guid VenueId { get; set; }

        [Required(ErrorMessage = "Event date is required")]
        [DataType(DataType.Date)]
        public DateTime EventDate { get; set; }

        [Required(ErrorMessage = "Start time is required")]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "End time is required")]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }

        [Required(ErrorMessage = "Total capacity is required")]
        [Range(1, 100000, ErrorMessage = "Capacity must be between 1 and 100,000")]
        public int TotalCapacity { get; set; }

        [StringLength(500, ErrorMessage = "Tags cannot exceed 500 characters")]
        public string? Tags { get; set; }

        [StringLength(50, ErrorMessage = "Age restriction cannot exceed 50 characters")]
        public string? AgeRestriction { get; set; } = "All Ages";

        public bool IsMultiDay { get; set; } = false;

        [DataType(DataType.DateTime)]
        public DateTime? TicketSalesStart { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? TicketSalesEnd { get; set; }

        [StringLength(100, ErrorMessage = "Refund policy cannot exceed 100 characters")]
        public string RefundPolicy { get; set; } = "No Refunds";

        public IFormFile? ImageFile { get; set; }
        public string? ImageUrl { get; set; }

        public List<EventPriceViewModel> EventPrices { get; set; } = new List<EventPriceViewModel>();

        // Navigation properties for dropdowns
        public List<Category> Categories { get; set; } = new List<Category>();
        public List<Venue> Venues { get; set; } = new List<Venue>();
    }

    public class EventPriceViewModel
    {
        public Guid? Id { get; set; }
        [Required(ErrorMessage = "Price category name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Price category must be between 2 and 100 characters")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Range(0, 999999.99, ErrorMessage = "Price must be between 0 and 999,999.99")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stock quantity is required")]
        [Range(1, 100000, ErrorMessage = "Stock must be between 1 and 100,000")]
        public int Stock { get; set; }

        public bool IsActive { get; set; } = true;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        public string? PriceType { get; set; } = "Standard"; // Standard, Early Bird, VIP, Group, Student, etc.
    }

}
