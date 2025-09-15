namespace online_event_booking_system.Models
{
    public class RecentReport
    {
        public string ReportName { get; set; }
        public string Type { get; set; }
        public string Format { get; set; }
        public DateTime Generated { get; set; }
        public string FileName { get; set; }
    }
}
