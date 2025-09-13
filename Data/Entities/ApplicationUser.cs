using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace online_event_booking_system.Data.Entities
{
    /// <summary>
    /// Application user class extending IdentityUser to include additional properties.
    /// </summary>
    public class ApplicationUser: IdentityUser
    {
        [Required(ErrorMessage = "Full Name is required")]
        [StringLength(100, ErrorMessage = "Full Name cannot exceed 100 characters")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact Number is required")]
        [StringLength(15, ErrorMessage = "Contact Number cannot exceed 15 characters")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        public string ContactNumber { get; set; } = string.Empty;

        public string? StripeCustomerId { get; set; }

        [StringLength(20, ErrorMessage = "NIC cannot exceed 20 characters")]
        public string? NIC { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string? Address { get; set; }

        [StringLength(100, ErrorMessage = "Organization Name cannot exceed 100 characters")]
        public string? OrganizationName { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime? LastLogin { get; set; }
        public DateTime? DeletedAt { get; set; }

        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();

    }
}
