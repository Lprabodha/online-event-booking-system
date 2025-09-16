using online_event_booking_system.Data.Entities;

namespace online_event_booking_system.Models.View_Models
{
    public class OrganizerDashboardViewModel
    {
        public int TotalEvents { get; set; }
        public int PublishedEvents { get; set; }
        public int TotalBookings { get; set; }
        public decimal TotalRevenue { get; set; }

        public List<Event> RecentEvents { get; set; } = new();
        public List<Event> TopSellingEvents { get; set; } = new();
        public List<Discount> ActiveDiscounts { get; set; } = new();

        // Default (7d) for initial render
        public List<string> SalesLabels { get; set; } = new();
        public List<decimal> SalesData { get; set; } = new();

        // Precomputed alternate ranges to avoid API calls from the view
        public List<string> SalesLabels7d { get; set; } = new();
        public List<decimal> SalesData7d { get; set; } = new();
        public List<string> SalesLabels30d { get; set; } = new();
        public List<decimal> SalesData30d { get; set; } = new();
    }
}


