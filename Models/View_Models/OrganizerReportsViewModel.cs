using online_event_booking_system.Data.Entities;

namespace online_event_booking_system.Models.View_Models
{
    public class OrganizerReportsViewModel
    {
        // Key metrics
        public decimal TotalRevenue { get; set; }
        public int TicketsSold { get; set; }
        public int NewCustomers { get; set; }
        public decimal AverageTicketPrice { get; set; }

        // Revenue chart
        public List<string> RevenueLabels7d { get; set; } = new();
        public List<decimal> RevenueData7d { get; set; } = new();
        public List<string> RevenueLabels30d { get; set; } = new();
        public List<decimal> RevenueData30d { get; set; } = new();

        // Ticket chart
        public List<string> TicketLabels7d { get; set; } = new();
        public List<int> TicketData7d { get; set; } = new();
        public List<string> TicketLabelsWeekly { get; set; } = new();
        public List<int> TicketDataWeekly { get; set; } = new();

        // Top events table
        public List<TopEventRow> TopEvents { get; set; } = new();

        // Sales by category
        public List<CategorySalesRow> CategorySales { get; set; } = new();

        // Recent activity
        public List<ActivityItem> RecentActivities { get; set; } = new();

        // Quick stats
        public decimal AverageSaleTimeDays { get; set; }
        public decimal CustomerSatisfactionPercent { get; set; }
        public decimal MonthlyRecurring { get; set; }
    }

    public class TopEventRow
    {
        public Guid EventId { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public int TicketsSold { get; set; }
        public decimal Revenue { get; set; }
        public string ConversionText { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class CategorySalesRow
    {
        public string CategoryName { get; set; } = string.Empty;
        public int Percentage { get; set; }
        public string ColorClass { get; set; } = "bg-purple-500";
    }

    public class ActivityItem
    {
        public string Description { get; set; } = string.Empty;
        public string WhenText { get; set; } = string.Empty;
    }
}


