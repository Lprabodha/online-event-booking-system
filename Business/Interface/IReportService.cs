using online_event_booking_system.Models;
using online_event_booking_system.Data.Entities;

namespace online_event_booking_system.Business.Interface
{
    public interface IReportService
    {
        Task<byte[]> GenerateReportAsync(string reportType, string format, DateTime? dateFrom, DateTime? dateTo, string category = null, string organizer = null, string role = null);
        Task<IEnumerable<RecentReport>> GetRecentReportsAsync(DateTime? dateFrom, DateTime? dateTo);
        Task<IEnumerable<ApplicationUser>> GetUsersAsync(DateTime? dateFrom, DateTime? dateTo, string role = null);
        Task<IEnumerable<Event>> GetEventsAsync(DateTime? dateFrom, DateTime? dateTo, string category = null, string organizer = null);
    }
}
