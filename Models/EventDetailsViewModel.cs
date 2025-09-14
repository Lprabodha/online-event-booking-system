using online_event_booking_system.Data.Entities;

namespace online_event_booking_system.Models
{
    public class EventDetailsViewModel
    {
        public Event Event { get; set; } = default!;
        public List<Event> RelatedEvents { get; set; } = new List<Event>();
    }
}
