using online_event_booking_system.Data.Entities;

namespace online_event_booking_system.Repository.Interface
{
    public interface IReportRepository
    {
        Task<IEnumerable<Event>> GetEventsAsync(DateTime? from, DateTime? to, string category = null, string status = null);
        Task<IEnumerable<ApplicationUser>> GetUsersAsync(DateTime? from, DateTime? to);
        Task<IEnumerable<ApplicationUser>> GetOrganizersAsync(DateTime? from, DateTime? to);
        Task<IEnumerable<ApplicationUser>> GetCustomersAsync(DateTime? from, DateTime? to);
    }
}
