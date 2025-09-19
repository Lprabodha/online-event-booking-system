using System.ComponentModel.DataAnnotations;

namespace online_event_booking_system.Models.View_Models
{
    public class OrganizerEmailViewModel
    {
        [Required]
        [StringLength(150)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [StringLength(10000)]
        public string Body { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select at least one recipient")]
        public List<string> SelectedUserIds { get; set; } = new List<string>();

        public List<RecipientOption> Recipients { get; set; } = new List<RecipientOption>();
    }

    public class RecipientOption
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}


