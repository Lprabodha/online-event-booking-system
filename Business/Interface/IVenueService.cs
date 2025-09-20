using online_event_booking_system.Data.Entities;

namespace online_event_booking_system.Business.Interface;

public interface IVenueService
{
    /// <summary>
    /// Get all venues
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<Venue>> GetAllVenuesAsync();
    /// <summary>
    /// Get venue by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<Venue> GetVenueByIdAsync(Guid id);
    /// <summary>
    /// Create a new venue
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    Task<Venue> CreateVenueAsync(Venue model);
    /// <summary>
    /// Update an existing venue
    /// </summary>
    /// <param name="id"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    Task<Venue> UpdateVenueAsync(Guid id, Venue model);
    /// <summary>
    /// Delete a venue
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<bool> DeleteVenueAsync(Guid id);
}