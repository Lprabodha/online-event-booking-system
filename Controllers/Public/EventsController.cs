using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using online_event_booking_system.Business.Interface;
using online_event_booking_system.Models;
using online_event_booking_system.Services;

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
        public async Task<IActionResult> Index()
        {
            try
            {
                var events = await _eventService.GetPublishedEventsAsync();
                
                // Process event images to convert S3 keys to URLs
                foreach (var eventItem in events)
                {
                    if (!string.IsNullOrEmpty(eventItem.Image))
                    {
                        eventItem.Image = await _s3Service.GetImageUrlAsync(eventItem.Image);
                    }
                }
                
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
