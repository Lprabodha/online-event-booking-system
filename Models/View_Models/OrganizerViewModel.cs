using System.ComponentModel.DataAnnotations;

namespace online_event_booking_system.Models.View_Models
{
    public class OrganizerViewModel
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "Full Name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Full Name must be between 2 and 100 characters")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [StringLength(256, ErrorMessage = "Email cannot exceed 256 characters")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "NIC Number is required")]
        [StringLength(12, MinimumLength = 10, ErrorMessage = "NIC Number must be between 10 and 12 characters")]
        [RegularExpression(@"^[0-9]{9}[vVxX]?$|^[0-9]{12}$", ErrorMessage = "Please enter a valid NIC number")]
        [Display(Name = "NIC Number")]
        public string NIC { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone Number is required")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [StringLength(15, MinimumLength = 10, ErrorMessage = "Phone Number must be between 10 and 15 characters")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Organization Name is required")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Organization Name must be between 2 and 200 characters")]
        [Display(Name = "Organization Name")]
        public string OrganizationName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Address must be between 10 and 500 characters")]
        [Display(Name = "Address")]
        public string Address { get; set; } = string.Empty;

        [Display(Name = "Is Verified")]
        public bool IsVerified { get; set; } = false;

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;
    }
}
