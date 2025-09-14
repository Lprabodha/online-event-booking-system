using online_event_booking_system.Data.Entities;

namespace online_event_booking_system.Repository.Interface;

public interface IVenueRepository
{
    Task<IEnumerable<Venue>> GetAllAsync();
    Task<Venue> GetByIdAsync(Guid id);
    Task<Venue> AddAsync(Venue venue);
    Task<Venue> UpdateAsync(Venue venue);
    Task<bool> DeleteAsync(Guid id);
}