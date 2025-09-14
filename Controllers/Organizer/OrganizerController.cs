using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using online_event_booking_system.Business.Interface;
using System.Security.Claims;
using online_event_booking_system.Models;
using online_event_booking_system.Models.View_Models;
using online_event_booking_system.Data.Entities;

namespace online_event_booking_system.Controllers.Organizer
{
    [Authorize(Roles = "Organizer")]
    public class OrganizerController : Controller
    {
        private readonly IEventService _eventService;
        private readonly ILogger<OrganizerController> _logger;

        public OrganizerController(IEventService eventService, ILogger<OrganizerController> logger)
        {
            _eventService = eventService;
            _logger = logger;
        }

        [HttpGet("organizer")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("organizer/events")]
        public async Task<IActionResult> Events(int page = 1, int pageSize = 10)
        {
            try
            {
                var organizerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
                var (events, totalCount, totalPages) = await _eventService.GetEventsByOrganizerWithPaginationAsync(organizerId, page, pageSize);
                
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalCount = totalCount;
                ViewBag.PageSize = pageSize;
                
                return View(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading events");
                TempData["ErrorMessage"] = "An error occurred while loading events. Please try again.";
                return View(new List<Data.Entities.Event>());
            }
        }

        [HttpGet("organizer/reports")]
        public IActionResult Reports()
        {
            return View();
        }

        [HttpGet("organizer/payouts")]
        public IActionResult Payouts()
        {
            return View();
        }

        [HttpGet("organizer/support")]
        public IActionResult Support()
        {
            return View();
        }

        [HttpGet("organizer/settings")]
        public IActionResult Settings()
        {
            return View();
        }

        [HttpGet("organizer/discounts")]
        public IActionResult Discounts()
        {
            var model = new DiscountViewModel
            {
                AvailableEvents = GetAvailableEvents()
            };
            return View(model);
        }

        [HttpPost("organizer/discounts")]
        [ValidateAntiForgeryToken]
        public IActionResult CreateDiscount(DiscountViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableEvents = GetAvailableEvents();
                return View("Discounts", model);
            }

            // In a real application, you would save the discount to the database here
            // For now, we'll just simulate success
            
            TempData["SuccessMessage"] = $"Discount code '{model.Code}' has been created successfully!";
            return RedirectToAction("Discounts");
        }

        [HttpGet("organizer/create-event")]
        public async Task<IActionResult> CreateEvent()
        {
            try
            {
                var model = await _eventService.GetCreateEventViewModelAsync();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create event page");
                TempData["ErrorMessage"] = "An error occurred while loading the create event page. Please try again.";
                return RedirectToAction("Events");
            }
        }

        [HttpPost("organizer/create-event")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(CreateEventViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Reload dropdown data
                    model.Categories = await _eventService.GetCategoriesAsync();
                    model.Venues = await _eventService.GetVenuesAsync();
                    return View(model);
                }

                // Validate event date is in the future
                if (model.EventDate < DateTime.Today)
                {
                    ModelState.AddModelError("EventDate", "Event date must be in the future");
                    model.Categories = await _eventService.GetCategoriesAsync();
                    model.Venues = await _eventService.GetVenuesAsync();
                    return View(model);
                }

                // Validate end time is after start time
                if (model.EndTime <= model.StartTime)
                {
                    ModelState.AddModelError("EndTime", "End time must be after start time");
                    model.Categories = await _eventService.GetCategoriesAsync();
                    model.Venues = await _eventService.GetVenuesAsync();
                    return View(model);
                }

                // Validate total capacity matches sum of ticket stocks
                var totalTicketStock = model.EventPrices.Sum(ep => ep.Stock);
                if (totalTicketStock != model.TotalCapacity)
                {
                    ModelState.AddModelError("TotalCapacity", "Total capacity must match the sum of all ticket stocks");
                    model.Categories = await _eventService.GetCategoriesAsync();
                    model.Venues = await _eventService.GetVenuesAsync();
                    return View(model);
                }

                var organizerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
                var createdEvent = await _eventService.CreateEventAsync(model, organizerId);

                TempData["SuccessMessage"] = $"Event '{createdEvent.Title}' has been created successfully!";
                return RedirectToAction("Events");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating event");
                TempData["ErrorMessage"] = $"An error occurred while creating the event. Please try again. Error: {ex.Message}";
                
                // Reload dropdown data
                model.Categories = await _eventService.GetCategoriesAsync();
                model.Venues = await _eventService.GetVenuesAsync();
                return View(model);
            }
        }

        [HttpGet("organizer/edit-event/{id}")]
        public async Task<IActionResult> EditEvent(Guid id)
        {
            try
            {
                var eventEntity = await _eventService.GetEventByIdAsync(id);
                if (eventEntity == null)
                {
                    TempData["ErrorMessage"] = "Event not found.";
                    return RedirectToAction("Events");
                }

                // Check if user owns this event
                var organizerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
                if (eventEntity.OrganizerId != organizerId)
                {
                    TempData["ErrorMessage"] = "You don't have permission to edit this event.";
                    return RedirectToAction("Events");
                }

                var model = new CreateEventViewModel
                {
                    Title = eventEntity.Title,
                    Description = eventEntity.Description,
                    CategoryId = eventEntity.CategoryId,
                    VenueId = eventEntity.VenueId,
                    EventDate = eventEntity.EventDate,
                    StartTime = eventEntity.EventTime.TimeOfDay,
                    EndTime = eventEntity.EventTime.AddHours(2).TimeOfDay, // Assuming 2-hour default duration
                    TotalCapacity = eventEntity.TotalCapacity,
                    Tags = eventEntity.Tags,
                    AgeRestriction = eventEntity.AgeRestriction,
                    IsMultiDay = eventEntity.IsMultiDay,
                    TicketSalesStart = eventEntity.TicketSalesStart,
                    TicketSalesEnd = eventEntity.TicketSalesEnd,
                    RefundPolicy = eventEntity.RefundPolicy,
                    ImageUrl = eventEntity.Image,
                    Categories = await _eventService.GetCategoriesAsync(),
                    Venues = await _eventService.GetVenuesAsync(),
                    EventPrices = eventEntity.Prices.Select(p => new EventPriceViewModel
                    {
                        Category = p.Category,
                        Price = p.Price,
                        Stock = p.Stock,
                        IsActive = p.IsActive,
                        Description = p.Description,
                        PriceType = p.PriceType
                    }).ToList()
                };

                // Calculate analytics data
                var totalTicketsSold = eventEntity.Bookings?.Sum(b => b.Tickets?.Count ?? 0) ?? 0;
                var totalRevenue = eventEntity.Bookings?.SelectMany(b => b.Tickets).Sum(t => t.Payment?.Amount ?? 0) ?? 0;
                var capacityUsedPercentage = eventEntity.TotalCapacity > 0 ? (totalTicketsSold * 100.0 / eventEntity.TotalCapacity) : 0;
                var remainingTickets = eventEntity.TotalCapacity - totalTicketsSold;

                ViewBag.EventEntity = eventEntity;
                ViewBag.TotalTicketsSold = totalTicketsSold;
                ViewBag.TotalRevenue = totalRevenue;
                ViewBag.CapacityUsedPercentage = capacityUsedPercentage;
                ViewBag.RemainingTickets = remainingTickets;

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit event page");
                TempData["ErrorMessage"] = "An error occurred while loading the event. Please try again.";
                return RedirectToAction("Events");
            }
        }

        [HttpPost("organizer/edit-event/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEvent(Guid id, CreateEventViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    model.Categories = await _eventService.GetCategoriesAsync();
                    model.Venues = await _eventService.GetVenuesAsync();
                    return View(model);
                }

                var organizerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
                await _eventService.UpdateEventAsync(id, model, organizerId);

                TempData["SuccessMessage"] = $"Event '{model.Title}' has been updated successfully!";
                return RedirectToAction("Events");
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Events");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating event");
                TempData["ErrorMessage"] = "An error occurred while updating the event. Please try again.";
                
                model.Categories = await _eventService.GetCategoriesAsync();
                model.Venues = await _eventService.GetVenuesAsync();
                return View(model);
            }
        }

        [HttpPost("organizer/delete-event/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvent(Guid id)
        {
            try
            {
                var organizerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
                var result = await _eventService.DeleteEventAsync(id, organizerId);

                if (result)
                {
                    TempData["SuccessMessage"] = "Event has been deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Event not found or you don't have permission to delete it.";
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting event");
                TempData["ErrorMessage"] = "An error occurred while deleting the event. Please try again.";
            }

            return RedirectToAction("Events");
        }

        [HttpPost("organizer/publish-event/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PublishEvent(Guid id)
        {
            try
            {
                var organizerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
                var result = await _eventService.PublishEventAsync(id, organizerId);

                if (result)
                {
                    TempData["SuccessMessage"] = "Event has been published successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Event not found or you don't have permission to publish it.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing event");
                TempData["ErrorMessage"] = "An error occurred while publishing the event. Please try again.";
            }

            return RedirectToAction("Events");
        }

        [HttpPost("organizer/unpublish-event/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnpublishEvent(Guid id)
        {
            try
            {
                var organizerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
                var result = await _eventService.UnpublishEventAsync(id, organizerId);

                if (result)
                {
                    TempData["SuccessMessage"] = "Event has been unpublished successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Event not found or you don't have permission to unpublish it.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unpublishing event");
                TempData["ErrorMessage"] = "An error occurred while unpublishing the event. Please try again.";
            }

            return RedirectToAction("Events");
        }


        private List<EventOption> GetAvailableEvents()
        {
            // In a real application, this would come from your database
            return new List<EventOption>
            {
                new EventOption { Id = Guid.NewGuid(), Name = "Summer Music Festival", EventDate = DateTime.Now.AddDays(30) },
                new EventOption { Id = Guid.NewGuid(), Name = "Tech Conference 2024", EventDate = DateTime.Now.AddDays(45) },
                new EventOption { Id = Guid.NewGuid(), Name = "Art Exhibition", EventDate = DateTime.Now.AddDays(60) },
                new EventOption { Id = Guid.NewGuid(), Name = "Food & Wine Tasting", EventDate = DateTime.Now.AddDays(75) },
                new EventOption { Id = Guid.NewGuid(), Name = "Comedy Night", EventDate = DateTime.Now.AddDays(90) }
            };
        }
    }
}
