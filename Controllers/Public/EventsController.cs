using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using online_event_booking_system.Business.Interface;
using online_event_booking_system.Models;
using online_event_booking_system.Services;
using System.Linq;

namespace online_event_booking_system.Controllers.Public
{
    public class EventsController : Controller
    {
        private readonly IEventService _eventService;
        private readonly ILogger<EventsController> _logger;
        private readonly IS3Service _s3Service;

        public EventsController(IEventService eventService, ILogger<EventsController> logger, IS3Service s3Service)
        {
            _eventService = eventService;
            _logger = logger;
            _s3Service = s3Service;
        }

        [AllowAnonymous]
        [HttpGet("events")]
        public async Task<IActionResult> Index(string? search, string? category, string? dateFilter, string? sortBy)
        {
            try
            {
                // Get all published events
                var events = await _eventService.GetPublishedEventsAsync();
                
                // Apply search filter
                if (!string.IsNullOrEmpty(search))
                {
                    events = events.Where(e => 
                        e.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        (e.Description?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (e.Venue?.Name?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (e.Category?.Name?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                    ).ToList();
                }

                // Apply category filter
                if (!string.IsNullOrEmpty(category) && Guid.TryParse(category, out var categoryId))
                {
                    events = events.Where(e => e.CategoryId == categoryId).ToList();
                }

                // Apply date filter
                var now = DateTime.UtcNow;
                switch (dateFilter?.ToLower())
                {
                    case "today":
                        events = events.Where(e => e.EventDate.Date == now.Date).ToList();
                        break;
                    case "tomorrow":
                        events = events.Where(e => e.EventDate.Date == now.AddDays(1).Date).ToList();
                        break;
                    case "thisweek":
                        var weekStart = now.Date.AddDays(-(int)now.DayOfWeek);
                        var weekEnd = weekStart.AddDays(7);
                        events = events.Where(e => e.EventDate.Date >= weekStart && e.EventDate.Date < weekEnd).ToList();
                        break;
                    case "thismonth":
                        events = events.Where(e => e.EventDate.Month == now.Month && e.EventDate.Year == now.Year).ToList();
                        break;
                    case "upcoming":
                        events = events.Where(e => e.EventDate > now).ToList();
                        break;
                }

                // Apply sorting
                switch (sortBy?.ToLower())
                {
                    case "date":
                        events = events.OrderBy(e => e.EventDate).ToList();
                        break;
                    case "date_desc":
                        events = events.OrderByDescending(e => e.EventDate).ToList();
                        break;
                    case "title":
                        events = events.OrderBy(e => e.Title).ToList();
                        break;
                    case "price":
                        events = events.OrderBy(e => e.Prices?.Where(p => p.IsActive).Min(p => p.Price) ?? 0).ToList();
                        break;
                    case "price_desc":
                        events = events.OrderByDescending(e => e.Prices?.Where(p => p.IsActive).Min(p => p.Price) ?? 0).ToList();
                        break;
                    default:
                        events = events.OrderByDescending(e => e.CreatedAt).ToList();
                        break;
                }
                
                // Process event images to convert S3 keys to URLs
                foreach (var eventItem in events)
                {
                    if (!string.IsNullOrEmpty(eventItem.Image))
                    {
                        eventItem.Image = await _s3Service.GetImageUrlAsync(eventItem.Image);
                    }
                }

                // Get categories for filter dropdown
                var categories = await _eventService.GetCategoriesAsync();
                
                // Pass filter values to view
                ViewBag.SearchTerm = search;
                ViewBag.SelectedCategory = category;
                ViewBag.SelectedDateFilter = dateFilter;
                ViewBag.SelectedSortBy = sortBy;
                ViewBag.Categories = categories;
                
                return View(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching events for events page");
                return View(new List<online_event_booking_system.Data.Entities.Event>());
            }
        }

        [HttpGet("events/{id}")]
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var eventEntity = await _eventService.GetEventByIdAsync(id);
                
                if (eventEntity == null || !eventEntity.IsPublished)
                {
                    return NotFound();
                }

                // Debug logging to check if description is present
                _logger.LogInformation("Event {EventId} - Title: {Title}, Description: {Description}", 
                    id, eventEntity.Title, eventEntity.Description ?? "NULL");

                // Process event image to convert S3 key to URL
                if (!string.IsNullOrEmpty(eventEntity.Image))
                {
                    eventEntity.Image = await _s3Service.GetImageUrlAsync(eventEntity.Image);
                }

                // Get related events
                var relatedEvents = await _eventService.GetRelatedEventsAsync(id, 3);
                
                // Process related events images
                foreach (var relatedEvent in relatedEvents)
                {
                    if (!string.IsNullOrEmpty(relatedEvent.Image))
                    {
                        relatedEvent.Image = await _s3Service.GetImageUrlAsync(relatedEvent.Image);
                    }
                }

                // Create a view model to pass both event and related events
                var viewModel = new EventDetailsViewModel
                {
                    Event = eventEntity,
                    RelatedEvents = relatedEvents
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching event details for event {EventId}", id);
                return NotFound();
            }
        }
    }
}
