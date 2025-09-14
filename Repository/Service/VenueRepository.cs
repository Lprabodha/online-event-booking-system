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

    public async Task<IEnumerable<Venue>> GetAllAsync()
    {
        return await _context.Venues.ToListAsync();
    }

    public async Task<Venue> GetByIdAsync(Guid id)
    {
        return await _context.Venues.FindAsync(id);
    }

    public async Task<Venue> AddAsync(Venue venue)
    {
        await _context.Venues.AddAsync(venue);
        await _context.SaveChangesAsync();
        return venue;
    }

    public async Task<Venue> UpdateAsync(Venue venue)
    {
        _context.Venues.Update(venue);
        await _context.SaveChangesAsync();
        return venue;
    }

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