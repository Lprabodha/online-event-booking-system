using online_event_booking_system.Data.Entities;

namespace online_event_booking_system.Repository.Interface;

public interface IVenueRepository
{
    /// <summary>
    /// Get all venues in the system
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<Venue>> GetAllAsync();
    /// <summary>
    /// Get venue by its unique identifier
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<Venue> GetByIdAsync(Guid id);
    /// <summary>
    /// Create a new venue
    /// </summary>
    /// <param name="venue"></param>
    /// <returns></returns>
    Task<Venue> AddAsync(Venue venue);
    /// <summary>
    /// Update an existing venue
    /// </summary>
    /// <param name="venue"></param>
    /// <returns></returns>
    Task<Venue> UpdateAsync(Venue venue);
    /// <summary>
    /// Delete a venue
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<bool> DeleteAsync(Guid id);
}