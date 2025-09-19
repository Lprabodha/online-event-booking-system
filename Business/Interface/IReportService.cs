using online_event_booking_system.Models;
using online_event_booking_system.Data.Entities;

namespace online_event_booking_system.Business.Interface
{
    public interface IReportService
    {
        /// <summary>
        /// Generate report based on the specified parameters
        /// </summary>
        /// <param name="reportType"></param>
        /// <param name="format"></param>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <param name="category"></param>
        /// <param name="organizer"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        Task<byte[]> GenerateReportAsync(string reportType, string format, DateTime? dateFrom, DateTime? dateTo, string category = null, string organizer = null, string role = null);
        /// <summary>
        /// Get recent reports generated within the specified date range
        /// </summary>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <returns></returns>
        Task<IEnumerable<RecentReport>> GetRecentReportsAsync(DateTime? dateFrom, DateTime? dateTo);
        /// <summary>
        /// Get users based on the specified parameters
        /// </summary>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        Task<IEnumerable<ApplicationUser>> GetUsersAsync(DateTime? dateFrom, DateTime? dateTo, string role = null);
        /// <summary>
        /// Get events based on the specified parameters
        /// </summary>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <param name="category"></param>
        /// <param name="organizer"></param>
        /// <returns></returns>
        Task<IEnumerable<Event>> GetEventsAsync(DateTime? dateFrom, DateTime? dateTo, string category = null, string organizer = null);

        /// <summary>
        /// Get revenue aggregates for admin reports
        /// </summary>
        Task<IEnumerable<RevenueReportRow>> GetRevenueAsync(DateTime? dateFrom, DateTime? dateTo, string category = null, string organizer = null);
    }
}
