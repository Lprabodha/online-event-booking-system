using online_event_booking_system.Models;

namespace online_event_booking_system.Business.Interface
{
    public interface IReportService
    {
        Task<byte[]> GenerateReportAsync(string reportType, string format, DateTime? dateFrom, DateTime? dateTo, string category, string status);
        Task<IEnumerable<RecentReport>> GetRecentReportsAsync(DateTime? dateFrom, DateTime? dateTo);
    }
}
