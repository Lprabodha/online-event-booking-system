using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using online_event_booking_system.Business.Interface;
using online_event_booking_system.Models;

namespace online_event_booking_system.Controllers.Public
{
    public class EventsController : Controller
    {
        private readonly IEventService _eventService;
        private readonly ILogger<EventsController> _logger;

        public EventsController(IEventService eventService, ILogger<EventsController> logger)
        {
            _eventService = eventService;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpGet("events")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var events = await _eventService.GetPublishedEventsAsync();
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

                // Get related events
                var relatedEvents = await _eventService.GetRelatedEventsAsync(id, 3);

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
