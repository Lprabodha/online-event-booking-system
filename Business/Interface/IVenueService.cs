using online_event_booking_system.Data.Entities;

namespace online_event_booking_system.Business.Interface;

public interface IVenueService
{
    Task<IEnumerable<Venue>> GetAllVenuesAsync();
    Task<Venue> GetVenueByIdAsync(Guid id);
    Task<Venue> CreateVenueAsync(Venue model);
    Task<Venue> UpdateVenueAsync(Guid id, Venue model);
    Task<bool> DeleteVenueAsync(Guid id);
}