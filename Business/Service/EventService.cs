using Microsoft.EntityFrameworkCore;
using online_event_booking_system.Business.Interface;
using online_event_booking_system.Data;
using online_event_booking_system.Data.Entities;
using online_event_booking_system.Models.View_Models;
using online_event_booking_system.Services;

namespace online_event_booking_system.Business.Service
{
    public class EventService : IEventService
    {
        private readonly ApplicationDbContext _context;
        private readonly IS3Service _s3Service;
        private readonly ILogger<EventService> _logger;

        public EventService(ApplicationDbContext context, IS3Service s3Service, ILogger<EventService> logger)
        {
            _context = context;
            _s3Service = s3Service;
            _logger = logger;
        }

        public async Task<Event> CreateEventAsync(CreateEventViewModel model, string organizerId)
        {
            try
            {
                var eventEntity = new Event
                {
                    Id = Guid.NewGuid(),
                    Title = model.Title,
                    Description = model.Description,
                    CategoryId = model.CategoryId,
                    VenueId = model.VenueId,
                    OrganizerId = organizerId,
                    EventDate = model.EventDate,
                    EventTime = model.EventDate.Add(model.StartTime),
                    IsPublished = false,
                    CreatedAt = DateTime.UtcNow,
                    // Additional fields
                    TotalCapacity = model.TotalCapacity,
                    Tags = model.Tags,
                    AgeRestriction = model.AgeRestriction,
                    IsMultiDay = model.IsMultiDay,
                    Status = "Draft",
                    TicketSalesStart = model.TicketSalesStart,
                    TicketSalesEnd = model.TicketSalesEnd,
                    RefundPolicy = model.RefundPolicy
                };

                // Handle image upload
                if (model.ImageFile != null)
                {
                    var imageKey = await _s3Service.UploadFileAsync(model.ImageFile, "events");
                    eventEntity.Image = await _s3Service.GetFileUrlAsync(imageKey);
                }
                else if (!string.IsNullOrEmpty(model.ImageUrl))
                {
                    eventEntity.Image = model.ImageUrl;
                }

                _context.Events.Add(eventEntity);

                // Add event prices
                foreach (var priceModel in model.EventPrices)
                {
                    var eventPrice = new EventPrice
                    {
                        Id = Guid.NewGuid(),
                        EventId = eventEntity.Id,
                        Category = priceModel.Category,
                        Price = priceModel.Price,
                        Stock = priceModel.Stock,
                        IsActive = priceModel.IsActive,
                        CreatedAt = DateTime.UtcNow,
                        // Additional fields
                        Description = priceModel.Description,
                        PriceType = priceModel.PriceType ?? "Standard"
                    };
                    _context.EventPrices.Add(eventPrice);
                }

                await _context.SaveChangesAsync();
                return eventEntity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating event");
                throw;
            }
        }

        public async Task<Event?> GetEventByIdAsync(Guid id)
        {
            return await _context.Events
                .Include(e => e.Category)
                .Include(e => e.Venue)
                .Include(e => e.Organizer)
                .Include(e => e.Prices)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<List<Event>> GetEventsByOrganizerAsync(string organizerId)
        {
            return await _context.Events
                .Include(e => e.Category)
                .Include(e => e.Venue)
                .Include(e => e.Prices)
                .Where(e => e.OrganizerId == organizerId && e.DeletedAt == null)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();
        }

        public async Task<(List<Event> Events, int TotalCount, int TotalPages)> GetEventsByOrganizerWithPaginationAsync(string organizerId, int page = 1, int pageSize = 10)
        {
            var query = _context.Events
                .Include(e => e.Category)
                .Include(e => e.Venue)
                .Include(e => e.Prices)
                .Where(e => e.OrganizerId == organizerId && e.DeletedAt == null);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var events = await query
                .OrderByDescending(e => e.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (events, totalCount, totalPages);
        }

        public async Task<Event> UpdateEventAsync(Guid id, CreateEventViewModel model, string organizerId)
        {
            var eventEntity = await _context.Events
                .FirstOrDefaultAsync(e => e.Id == id && e.OrganizerId == organizerId);

            if (eventEntity == null)
                throw new ArgumentException("Event not found or access denied");

            eventEntity.Title = model.Title;
            eventEntity.Description = model.Description;
            eventEntity.CategoryId = model.CategoryId;
            eventEntity.VenueId = model.VenueId;
            eventEntity.EventDate = model.EventDate;
            eventEntity.EventTime = model.EventDate.Add(model.StartTime);
            eventEntity.UpdatedAt = DateTime.UtcNow;
            
            // Update additional fields
            eventEntity.TotalCapacity = model.TotalCapacity;
            eventEntity.Tags = model.Tags;
            eventEntity.AgeRestriction = model.AgeRestriction;
            eventEntity.IsMultiDay = model.IsMultiDay;
            eventEntity.TicketSalesStart = model.TicketSalesStart;
            eventEntity.TicketSalesEnd = model.TicketSalesEnd;
            eventEntity.RefundPolicy = model.RefundPolicy;

            // Handle image update
            if (model.ImageFile != null)
            {
                var imageKey = await _s3Service.UploadFileAsync(model.ImageFile, "events");
                eventEntity.Image = await _s3Service.GetFileUrlAsync(imageKey);
            }
            else if (!string.IsNullOrEmpty(model.ImageUrl))
            {
                eventEntity.Image = model.ImageUrl;
            }

            // Update event prices
            var existingPrices = await _context.EventPrices
                .Where(ep => ep.EventId == id)
                .ToListAsync();

            _context.EventPrices.RemoveRange(existingPrices);

            foreach (var priceModel in model.EventPrices)
            {
                var eventPrice = new EventPrice
                {
                    Id = Guid.NewGuid(),
                    EventId = eventEntity.Id,
                    Category = priceModel.Category,
                    Price = priceModel.Price,
                    Stock = priceModel.Stock,
                    IsActive = priceModel.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    // Additional fields
                    Description = priceModel.Description,
                    PriceType = priceModel.PriceType ?? "Standard"
                };
                _context.EventPrices.Add(eventPrice);
            }

            await _context.SaveChangesAsync();
            return eventEntity;
        }

        public async Task<bool> DeleteEventAsync(Guid id, string organizerId)
        {
            var eventEntity = await _context.Events
                .FirstOrDefaultAsync(e => e.Id == id && e.OrganizerId == organizerId);

            if (eventEntity == null)
                return false;

            // Check if event has bookings
            var hasBookings = await _context.Bookings
                .AnyAsync(b => b.EventId == id);

            if (hasBookings)
                throw new InvalidOperationException("Cannot delete event with existing bookings");

            _context.Events.Remove(eventEntity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Event>> GetAllEventsAsync()
        {
            return await _context.Events
                .Include(e => e.Category)
                .Include(e => e.Venue)
                .Include(e => e.Organizer)
                .Include(e => e.Prices)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Event>> GetPublishedEventsAsync()
        {
            return await _context.Events
                .Include(e => e.Category)
                .Include(e => e.Venue)
                .Include(e => e.Organizer)
                .Include(e => e.Prices)
                .Where(e => e.IsPublished)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> PublishEventAsync(Guid id, string organizerId)
        {
            try
            {
                var eventEntity = await _context.Events
                    .FirstOrDefaultAsync(e => e.Id == id && e.OrganizerId == organizerId);

                if (eventEntity == null)
                    return false;

                // Check if event can be published (not cancelled or completed)
                if (eventEntity.Status == "Cancelled" || eventEntity.Status == "Completed")
                    return false;

                eventEntity.IsPublished = true;
                eventEntity.Status = "Published";
                eventEntity.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing event {EventId}", id);
                throw;
            }
        }

        public async Task<bool> UnpublishEventAsync(Guid id, string organizerId)
        {
            try
            {
                var eventEntity = await _context.Events
                    .FirstOrDefaultAsync(e => e.Id == id && e.OrganizerId == organizerId);

                if (eventEntity == null)
                    return false;

                eventEntity.IsPublished = false;
                eventEntity.Status = "Draft";
                eventEntity.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unpublishing event {EventId}", id);
                throw;
            }
        }

        public async Task<bool> CancelEventAsync(Guid id, string organizerId)
        {
            try
            {
                var eventEntity = await _context.Events
                    .FirstOrDefaultAsync(e => e.Id == id && e.OrganizerId == organizerId);

                if (eventEntity == null)
                    return false;

                // Check if event can be cancelled (not already completed)
                if (eventEntity.Status == "Completed")
                    return false;

                eventEntity.IsPublished = false;
                eventEntity.Status = "Cancelled";
                eventEntity.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling event {EventId}", id);
                throw;
            }
        }

        public async Task<bool> UpdateEventStatusAsync(Guid id, string organizerId, string status)
        {
            try
            {
                var eventEntity = await _context.Events
                    .FirstOrDefaultAsync(e => e.Id == id && e.OrganizerId == organizerId);

                if (eventEntity == null)
                    return false;

                // Validate status
                var validStatuses = new[] { "Draft", "Published", "Cancelled", "Completed" };
                if (!validStatuses.Contains(status))
                    return false;

                eventEntity.Status = status;
                eventEntity.IsPublished = status == "Published";
                eventEntity.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating event status {EventId} to {Status}", id, status);
                throw;
            }
        }

        public async Task<List<Category>> GetCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<List<Venue>> GetVenuesAsync()
        {
            return await _context.Venues
                .Where(v => v.IsActive)
                .OrderBy(v => v.Name)
                .ToListAsync();
        }

        public async Task<CreateEventViewModel> GetCreateEventViewModelAsync()
        {
            var categories = await GetCategoriesAsync();
            var venues = await GetVenuesAsync();

            return new CreateEventViewModel
            {
                Categories = categories,
                Venues = venues,
                EventPrices = new List<EventPriceViewModel>
                {
                    new EventPriceViewModel
                    {
                        Category = "General Admission",
                        Price = 0,
                        Stock = 100,
                        IsActive = true,
                        PriceType = "Standard"
                    }
                }
            };
        }
    }
}
