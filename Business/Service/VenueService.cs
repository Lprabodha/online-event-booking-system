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
        var venue = new Venue
        {
            Name = model.Name,
            Location = model.Location,
            Capacity = model.Capacity,
            Description = model.Description,
            ContactInfo = model.ContactInfo,
            Image = model.Image // Assuming this is a URL or file path
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

        venue.Name = model.Name;
        venue.Location = model.Location;
        venue.Capacity = model.Capacity;
        venue.Description = model.Description;
        venue.ContactInfo = model.ContactInfo;
        venue.Image = model.Image;
        venue.UpdatedAt = DateTime.UtcNow;

        return await _venueRepository.UpdateAsync(venue);
    }

    public async Task<bool> DeleteVenueAsync(Guid id)
    {
        return await _venueRepository.DeleteAsync(id);
    }
}