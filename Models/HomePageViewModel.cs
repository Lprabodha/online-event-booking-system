using online_event_booking_system.Data.Entities;

namespace online_event_booking_system.Models
{
    public class HomePageViewModel
    {
        public List<Category> Categories { get; set; } = new List<Category>();
        public List<Event> UpcomingEvents { get; set; } = new List<Event>();
        public List<Event> LatestEvents { get; set; } = new List<Event>();
        public List<Event> EventsThisWeek { get; set; } = new List<Event>();
    }
}
