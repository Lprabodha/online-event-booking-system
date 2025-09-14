using System.ComponentModel.DataAnnotations;

namespace online_event_booking_system.Models
{
    public class DiscountViewModel
    {
        [Required(ErrorMessage = "Discount code is required")]
        [StringLength(20, ErrorMessage = "Discount code cannot exceed 20 characters")]
        [RegularExpression(@"^[A-Z0-9]+$", ErrorMessage = "Discount code must contain only uppercase letters and numbers")]
        [Display(Name = "Discount Code")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Discount type is required")]
        [Display(Name = "Discount Type")]
        public DiscountType Type { get; set; }

        [Required(ErrorMessage = "Discount value is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Discount value must be greater than 0")]
        [Display(Name = "Discount Value")]
        public decimal Value { get; set; }

        [Required(ErrorMessage = "Event selection is required")]
        [Display(Name = "Event")]
        public Guid? EventId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Usage limit must be at least 1")]
        [Display(Name = "Usage Limit")]
        public int? UsageLimit { get; set; }

        [Display(Name = "Expiry Date")]
        public DateTime? ExpiryDate { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        // Navigation properties for dropdowns
        public List<EventOption> AvailableEvents { get; set; } = new List<EventOption>();
    }

    public enum DiscountType
    {
        Percentage = 1,
        FixedAmount = 2
    }

    public class EventOption
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
    }
}
