using online_event_booking_system.Data.Entities;

namespace online_event_booking_system.Repository.Interface
{
    public interface IReportRepository
    {
        /// <summary>
        /// Get events based on the specified parameters
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="category"></param>
        /// <param name="organizer"></param>
        /// <returns></returns>
        Task<IEnumerable<Event>> GetEventsAsync(DateTime? from, DateTime? to, string category = null, string organizer = null);
        /// <summary>
        /// Get users based on the specified parameters
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        Task<IEnumerable<ApplicationUser>> GetUsersAsync(DateTime? from, DateTime? to, string role = null);
        /// <summary>
        /// Get recent reports generated within the specified date range
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        Task<IEnumerable<ApplicationUser>> GetOrganizersAsync(DateTime? from, DateTime? to);
        /// <summary>
        /// Get recent reports generated within the specified date range
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        Task<IEnumerable<ApplicationUser>> GetCustomersAsync(DateTime? from, DateTime? to);
    }
}
