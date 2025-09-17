using online_event_booking_system.Data.Entities;

namespace online_event_booking_system.Models.View_Models
{
    public class EventBuyerViewModel
    {
        public string CustomerId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int TicketsPurchased { get; set; }
        public decimal AmountPaid { get; set; }
        public DateTime LastPurchaseAt { get; set; }
    }

    public class EventAnalyticsViewModel
    {
        public Guid EventId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string OrganizerId { get; set; } = string.Empty;
        public int TotalCapacity { get; set; }

        public int TicketsSold { get; set; }
        public int TicketsRemaining { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageTicketPrice { get; set; }

        public int DiscountCodesUsed { get; set; }
        public int ActiveDiscounts { get; set; }

        public List<EventBuyerViewModel> Buyers { get; set; } = new();
    }
}


