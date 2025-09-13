using online_event_booking_system.Business.Interface;
using online_event_booking_system.Data.Entities;
using online_event_booking_system.Repository.Interface;

namespace online_event_booking_system.Business.Service;

public class VenueService : IVenueService
{
    private readonly IVenueRepository _venueRepository;

    public VenueService(IVenueRepository venueRepository)
    {
        _venueRepository = venueRepository;
    }

    public async Task<IEnumerable<Venue>> GetAllVenuesAsync()
    {
        return await _venueRepository.GetAllAsync();
    }

    public async Task<Venue> GetVenueByIdAsync(Guid id)
    {
        return await _venueRepository.GetByIdAsync(id);
    }

    public async Task<Venue> CreateVenueAsync(Venue model)
    {
        // Additional business validation
        if (string.IsNullOrWhiteSpace(model.Name))
            throw new ArgumentException("Venue name cannot be empty");
        
        if (string.IsNullOrWhiteSpace(model.Location))
            throw new ArgumentException("Location cannot be empty");
        
        if (model.Capacity <= 0)
            throw new ArgumentException("Capacity must be greater than 0");

        // Check for duplicate venue names
        var existingVenues = await _venueRepository.GetAllAsync();
        if (existingVenues.Any(v => v.Name.Equals(model.Name, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("A venue with this name already exists");

        var venue = new Venue
        {
            Name = model.Name.Trim(),
            Location = model.Location.Trim(),
            Capacity = model.Capacity,
            Description = model.Description?.Trim(),
            ContactInfo = model.ContactInfo?.Trim(),
            Image = model.Image?.Trim()
        };
        return await _venueRepository.AddAsync(venue);
    }

    public async Task<Venue> UpdateVenueAsync(Guid id, Venue model)
    {
        var venue = await _venueRepository.GetByIdAsync(id);
        if (venue == null)
        {
            return null;
        }

        // Additional business validation
        if (string.IsNullOrWhiteSpace(model.Name))
            throw new ArgumentException("Venue name cannot be empty");
        
        if (string.IsNullOrWhiteSpace(model.Location))
            throw new ArgumentException("Location cannot be empty");
        
        if (model.Capacity <= 0)
            throw new ArgumentException("Capacity must be greater than 0");

        // Check for duplicate venue names (excluding current venue)
        var existingVenues = await _venueRepository.GetAllAsync();
        if (existingVenues.Any(v => v.Name.Equals(model.Name, StringComparison.OrdinalIgnoreCase) && v.Id != id))
            throw new InvalidOperationException("A venue with this name already exists");

        venue.Name = model.Name.Trim();
        venue.Location = model.Location.Trim();
        venue.Capacity = model.Capacity;
        venue.Description = model.Description?.Trim();
        venue.ContactInfo = model.ContactInfo?.Trim();
        venue.Image = model.Image?.Trim();
        venue.UpdatedAt = DateTime.UtcNow;

        return await _venueRepository.UpdateAsync(venue);
    }

    public async Task<bool> DeleteVenueAsync(Guid id)
    {
        return await _venueRepository.DeleteAsync(id);
    }
}