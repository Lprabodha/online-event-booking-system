using Microsoft.AspNetCore.Identity;

namespace online_event_booking_system.Data.Entities
{
    /// <summary>
    /// Application user class extending IdentityUser to include additional properties.
    /// </summary>
    public class ApplicationUser: IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public string? StripeCustomerId { get; set; }
        public string? NIC { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? OrganizationName { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? LastLogin { get; set; }

        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();

    }
}
