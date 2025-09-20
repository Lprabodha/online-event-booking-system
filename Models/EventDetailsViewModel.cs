using online_event_booking_system.Data.Entities;

namespace online_event_booking_system.Models
{
    /// <summary>
    /// ViewModel for displaying event details along with related events.
    /// </summary>
    public class EventDetailsViewModel
    {
        public Event Event { get; set; } = default!;
        public List<Event> RelatedEvents { get; set; } = new List<Event>();
    }
}
