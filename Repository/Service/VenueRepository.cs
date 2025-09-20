using Microsoft.EntityFrameworkCore;
using online_event_booking_system.Data;
using online_event_booking_system.Data.Entities;
using online_event_booking_system.Repository.Interface;

namespace online_event_booking_system.Repository.Service;

public class VenueRepository : IVenueRepository
{
    private readonly ApplicationDbContext _context;

    public VenueRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all venues in the system
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<Venue>> GetAllAsync()
    {
        return await _context.Venues.ToListAsync();
    }

    /// <summary>
    /// Get venue by its unique identifier
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<Venue> GetByIdAsync(Guid id)
    {
        return await _context.Venues.FindAsync(id);
    }

    /// <summary>
    /// Create a new venue
    /// </summary>
    /// <param name="venue"></param>
    /// <returns></returns>
    public async Task<Venue> AddAsync(Venue venue)
    {
        await _context.Venues.AddAsync(venue);
        await _context.SaveChangesAsync();
        return venue;
    }

    /// <summary>
    /// Update an existing venue
    /// </summary>
    /// <param name="venue"></param>
    /// <returns></returns>

    public async Task<Venue> UpdateAsync(Venue venue)
    {
        _context.Venues.Update(venue);
        await _context.SaveChangesAsync();
        return venue;
    }

    /// <summary>
    /// Delete a venue
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<bool> DeleteAsync(Guid id)
    {
        var venue = await _context.Venues.FindAsync(id);
        if (venue == null)
        {
            return false;
        }
        _context.Venues.Remove(venue);
        await _context.SaveChangesAsync();
        return true;
    }
}