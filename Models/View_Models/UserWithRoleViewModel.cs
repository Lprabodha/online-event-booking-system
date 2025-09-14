using online_event_booking_system.Data.Entities;

namespace online_event_booking_system.Models.View_Models
{
    public class UserWithRoleViewModel
    {
        public ApplicationUser User { get; set; }
        public string Role { get; set; }
    }
}
