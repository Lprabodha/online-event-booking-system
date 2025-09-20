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

        /// <summary>
        /// Initializes a new instance of the EventService class.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="s3Service"></param>
        /// <param name="logger"></param>
        public EventService(ApplicationDbContext context, IS3Service s3Service, ILogger<EventService> logger)
        {
            _context = context;
            _s3Service = s3Service;
            _logger = logger;
        }

        /// <summary>
        /// Create a new event
        /// </summary>
        /// <param name="model"></param>
        /// <param name="organizerId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
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
                    eventEntity.Image = imageKey; // Save the S3 bucket path/key, not presigned URL
                }
                else if (!string.IsNullOrEmpty(model.ImageUrl))
                {
                    eventEntity.Image = model.ImageUrl;
                }

                _context.Events.Add(eventEntity);

                // Normalize and add event prices safely
                var prices = (model.EventPrices ?? new List<EventPriceViewModel>())
                    .Where(p => p != null)
                    .Select(p => new EventPriceViewModel
                    {
                        Category = p.Category?.Trim() ?? string.Empty,
                        Price = p.Price,
                        Stock = p.Stock,
                        IsActive = p.IsActive,
                        Description = p.Description,
                        PriceType = string.IsNullOrWhiteSpace(p.PriceType) ? "Standard" : p.PriceType
                    })
                    .Where(p => !string.IsNullOrWhiteSpace(p.Category) && p.Stock > 0 && p.Price >= 0)
                    .ToList();

                if (!prices.Any())
                {
                    throw new ArgumentException("Please add at least one valid ticket type with name, price, and stock.");
                }

                foreach (var priceModel in prices)
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

        /// <summary>
        /// Get event by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Event?> GetEventByIdAsync(Guid id)
        {
            return await _context.Events
                .Include(e => e.Category)
                .Include(e => e.Venue)
                .Include(e => e.Organizer)
                .Include(e => e.Prices)
                .Include(e => e.Bookings)
                    .ThenInclude(b => b.Customer)
                .Include(e => e.Discounts)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        /// <summary>
        /// Get events by organizer
        /// </summary>
        /// <param name="organizerId"></param>
        /// <returns></returns>
        public async Task<List<Event>> GetEventsByOrganizerAsync(string organizerId)
        {
            return await _context.Events
                .Include(e => e.Category)
                .Include(e => e.Venue)
                .Include(e => e.Prices)
                .Include(e => e.Bookings)
                    .ThenInclude(b => b.Tickets)
                        .ThenInclude(t => t.Payment)
                .Where(e => e.OrganizerId == organizerId && e.DeletedAt == null)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get events by organizer with pagination
        /// </summary>
        /// <param name="organizerId"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Update an existing event
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <param name="organizerId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
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
                eventEntity.Image = imageKey; // Save the S3 bucket path/key, not presigned URL
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

            var prices = (model.EventPrices ?? new List<EventPriceViewModel>())
                .Where(p => p != null)
                .Select(p => new EventPriceViewModel
                {
                    Category = p.Category?.Trim() ?? string.Empty,
                    Price = p.Price,
                    Stock = p.Stock,
                    IsActive = p.IsActive,
                    Description = p.Description,
                    PriceType = string.IsNullOrWhiteSpace(p.PriceType) ? "Standard" : p.PriceType
                })
                .Where(p => !string.IsNullOrWhiteSpace(p.Category) && p.Stock > 0 && p.Price >= 0)
                .ToList();

            if (!prices.Any())
            {
                throw new ArgumentException("Please add at least one valid ticket type with name, price, and stock.");
            }

            foreach (var priceModel in prices)
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
        /// <summary>
        /// Delete an event
        /// </summary>
        /// <param name="id"></param>
        /// <param name="organizerId"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
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
        /// <summary>
        /// Get all events
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Get published events
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Publish an event
        /// </summary>
        /// <param name="id"></param>
        /// <param name="organizerId"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Unpublish an event
        /// </summary>
        /// <param name="id"></param>
        /// <param name="organizerId"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Cancel an event
        /// </summary>
        /// <param name="id"></param>
        /// <param name="organizerId"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Update Event Status
        /// </summary>
        /// <param name="id"></param>
        /// <param name="organizerId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Get all active categories
        /// </summary>
        /// <returns></returns>
        public async Task<List<Category>> GetCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Get all active venues
        /// </summary>
        /// <returns></returns>
        public async Task<List<Venue>> GetVenuesAsync()
        {
            return await _context.Venues
                .Where(v => v.IsActive)
                .OrderBy(v => v.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Get CreateEventViewModel with categories and venues
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Get upcoming events
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public async Task<List<Event>> GetUpcomingEventsAsync(int count = 6)
        {
            var now = DateTime.UtcNow;
            return await _context.Events
                .Include(e => e.Category)
                .Include(e => e.Venue)
                .Include(e => e.Prices)
                .Where(e => e.IsPublished && 
                           e.Status == "Published" && 
                           e.EventDate >= now && 
                           e.DeletedAt == null)
                .OrderBy(e => e.EventDate)
                .Take(count)
                .ToListAsync();
        }

        /// <summary>
        ///  
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public async Task<List<Event>> GetLatestEventsAsync(int count = 4)
        {
            return await _context.Events
                .Include(e => e.Category)
                .Include(e => e.Venue)
                .Include(e => e.Prices)
                .Where(e => e.IsPublished && 
                           e.Status == "Published" && 
                           e.DeletedAt == null)
                .OrderByDescending(e => e.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        /// <summary>
        /// Get related events by category, excluding the current event. If not enough events in the same category, fill with other categories.
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public async Task<List<Event>> GetRelatedEventsAsync(Guid eventId, int count = 3)
        {
            // First get the current event to find related events
            var currentEvent = await _context.Events
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (currentEvent == null)
                return new List<Event>();

            // Get related events by category, excluding the current event
            var relatedEvents = await _context.Events
                .Include(e => e.Category)
                .Include(e => e.Venue)
                .Include(e => e.Prices)
                .Where(e => e.IsPublished && 
                           e.Status == "Published" && 
                           e.DeletedAt == null &&
                           e.Id != eventId &&
                           e.CategoryId == currentEvent.CategoryId)
                .OrderByDescending(e => e.CreatedAt)
                .Take(count)
                .ToListAsync();

            // If we don't have enough events from the same category, get more from other categories
            if (relatedEvents.Count < count)
            {
                var additionalEvents = await _context.Events
                    .Include(e => e.Category)
                    .Include(e => e.Venue)
                    .Include(e => e.Prices)
                    .Where(e => e.IsPublished && 
                               e.Status == "Published" && 
                               e.DeletedAt == null &&
                               e.Id != eventId &&
                               !relatedEvents.Select(re => re.Id).Contains(e.Id))
                    .OrderByDescending(e => e.CreatedAt)
                    .Take(count - relatedEvents.Count)
                    .ToListAsync();

                relatedEvents.AddRange(additionalEvents);
            }

            return relatedEvents;
        }

        /// <summary>
        /// Get events happening this week (Sunday to Saturday)
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public async Task<List<Event>> GetEventsThisWeekAsync(int count = 6)
        {
            var now = DateTime.UtcNow;
            var startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(7);

            return await _context.Events
                .Include(e => e.Category)
                .Include(e => e.Venue)
                .Include(e => e.Prices)
                .Where(e => e.IsPublished && 
                           e.Status == "Published" && 
                           e.DeletedAt == null &&
                           e.EventDate >= startOfWeek &&
                           e.EventDate < endOfWeek)
                .OrderBy(e => e.EventDate)
                .Take(count)
                .ToListAsync();
        }

        /// <summary>
        /// Get events happening next week (Sunday to Saturday)
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public async Task<List<Event>> GetEventsNextWeekAsync(int count = 6)
        {
            var now = DateTime.UtcNow;
            var startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek);
            var startOfNextWeek = startOfWeek.AddDays(7);
            var endOfNextWeek = startOfNextWeek.AddDays(7);

            return await _context.Events
                .Include(e => e.Category)
                .Include(e => e.Venue)
                .Include(e => e.Prices)
                .Where(e => e.IsPublished &&
                           e.Status == "Published" &&
                           e.DeletedAt == null &&
                           e.EventDate >= startOfNextWeek &&
                           e.EventDate < endOfNextWeek)
                .OrderBy(e => e.EventDate)
                .Take(count)
                .ToListAsync();
        }

        /// <summary>
        /// Get event analytics including tickets sold, revenue, average ticket price, discount usage, and buyer details.
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="restrictToOrganizerId"></param>
        /// <returns></returns>

        public async Task<EventAnalyticsViewModel?> GetEventAnalyticsAsync(Guid eventId, string? restrictToOrganizerId = null)
        {
            var query = _context.Events
                .Include(e => e.Bookings)
                    .ThenInclude(b => b.Customer)
                .Include(e => e.Bookings)
                    .ThenInclude(b => b.Tickets)
                        .ThenInclude(t => t.Payment)
                .Include(e => e.Discounts)
                .AsQueryable();

            query = query.Where(e => e.Id == eventId);
            if (!string.IsNullOrEmpty(restrictToOrganizerId))
            {
                query = query.Where(e => e.OrganizerId == restrictToOrganizerId);
            }

            var ev = await query.FirstOrDefaultAsync();
            if (ev == null)
                return null;

            var confirmedBookings = ev.Bookings?.Where(b => b.Status == "Confirmed").ToList() ?? new List<Booking>();
            var tickets = confirmedBookings.SelectMany(b => b.Tickets ?? new List<Ticket>()).ToList();
            var paidTickets = tickets.Where(t => t.Payment != null && t.IsPaid).ToList();
            var ticketsSold = paidTickets.Count;
            var totalRevenue = paidTickets.Sum(t => t.Payment!.Amount);
            var avgPrice = ticketsSold > 0 ? Math.Round(totalRevenue / ticketsSold, 2) : 0m;

            var buyers = confirmedBookings
                .GroupBy(b => new { b.CustomerId, b.Customer.FullName, b.Customer.Email })
                .Select(g => new EventBuyerViewModel
                {
                    CustomerId = g.Key.CustomerId,
                    CustomerName = g.Key.FullName ?? "Customer",
                    Email = g.Key.Email ?? string.Empty,
                    TicketsPurchased = g.SelectMany(b => b.Tickets ?? new List<Ticket>()).Count(),
                    AmountPaid = g.SelectMany(b => b.Tickets ?? new List<Ticket>())
                                   .Where(t => t.Payment != null)
                                   .Sum(t => t.Payment!.Amount),
                    LastPurchaseAt = g.Max(b => b.CreatedAt)
                })
                .OrderByDescending(b => b.AmountPaid)
                .ToList();

            var model = new EventAnalyticsViewModel
            {
                EventId = ev.Id,
                Title = ev.Title,
                OrganizerId = ev.OrganizerId,
                TotalCapacity = ev.TotalCapacity,
                TicketsSold = ticketsSold,
                TicketsRemaining = Math.Max(0, ev.TotalCapacity - ticketsSold),
                TotalRevenue = totalRevenue,
                AverageTicketPrice = avgPrice,
                DiscountCodesUsed = ev.Discounts?.Sum(d => d.UsedCount) ?? 0,
                ActiveDiscounts = ev.Discounts?.Count(d => d.IsActive) ?? 0,
                Buyers = buyers
            };

            return model;
        }
    }
}
