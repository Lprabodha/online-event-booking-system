using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using online_event_booking_system.Business.Interface;
using System.Security.Claims;
using online_event_booking_system.Models;
using online_event_booking_system.Models.View_Models;
using online_event_booking_system.Data.Entities;
using online_event_booking_system.Services;

namespace online_event_booking_system.Controllers.Organizer
{
    [Authorize(Roles = "Organizer")]
    public class OrganizerController : Controller
    {
        private readonly IEventService _eventService;
        private readonly IDiscountService _discountService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<OrganizerController> _logger;
        private readonly IS3Service _s3Service;

        public OrganizerController(
            IEventService eventService, 
            IDiscountService discountService, 
            UserManager<ApplicationUser> userManager,
            ILogger<OrganizerController> logger,
            IS3Service s3Service)
        {
            _eventService = eventService;
            _discountService = discountService;
            _userManager = userManager;
            _logger = logger;
            _s3Service = s3Service;
        }
        [HttpGet("organizer")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var organizerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
                
                // Get dashboard statistics
                var events = await _eventService.GetEventsByOrganizerAsync(organizerId);
                var totalEvents = events.Count();
                var publishedEvents = events.Count(e => e.IsPublished);
                var totalBookings = events.SelectMany(e => e.Bookings ?? new List<Booking>()).Count();
                var totalRevenue = events.SelectMany(e => e.Bookings ?? new List<Booking>())
                    .SelectMany(b => b.Tickets ?? new List<Ticket>())
                    .Where(t => t.Payment != null)
                    .Sum(t => t.Payment.Amount);

                ViewBag.TotalEvents = totalEvents;
                ViewBag.PublishedEvents = publishedEvents;
                ViewBag.TotalBookings = totalBookings;
                ViewBag.TotalRevenue = totalRevenue;

                // Pre-compute initial sales chart data for faster first paint (default 7d)
                var endDate = DateTime.UtcNow;
                var startDate = endDate.AddDays(-7).Date;
                var labels = new List<string>();
                var salesData = new List<decimal>();

                for (var date = startDate; date <= endDate.Date; date = date.AddDays(1))
                {
                    var daySales = events
                        .SelectMany(e => e.Bookings ?? new List<Booking>())
                        .Where(b => b.CreatedAt.Date == date && b.Status == "Confirmed")
                        .SelectMany(b => b.Tickets ?? new List<Ticket>())
                        .Where(t => t.Payment != null)
                        .Sum(t => t.Payment.Amount);

                    labels.Add(date.ToString("MMM dd"));
                    salesData.Add(daySales);
                }

                ViewBag.SalesLabels = labels;
                ViewBag.SalesData = salesData;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading organizer dashboard");
                return View();
            }
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

        [HttpGet("organizer/api/sales-data")]
        public async Task<IActionResult> GetSalesData(string period = "7d")
        {
            try
            {
                var organizerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
                var events = await _eventService.GetEventsByOrganizerAsync(organizerId);
                
                var endDate = DateTime.UtcNow;
                var startDate = period switch
                {
                    "7d" => endDate.AddDays(-7),
                    "30d" => endDate.AddDays(-30),
                    "90d" => endDate.AddDays(-90),
                    _ => endDate.AddDays(-7)
                };

                // Get sales data grouped by day
                var salesData = new List<object>();
                var labels = new List<string>();
                
                for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
                {
                    var daySales = events
                        .SelectMany(e => e.Bookings ?? new List<Booking>())
                        .Where(b => b.CreatedAt.Date == date && b.Status == "Confirmed")
                        .SelectMany(b => b.Tickets ?? new List<Ticket>())
                        .Where(t => t.Payment != null)
                        .Sum(t => t.Payment.Amount);

                    salesData.Add(daySales);
                    labels.Add(date.ToString("MMM dd"));
                }

                return Json(new
                {
                    labels = labels,
                    data = salesData,
                    totalSales = salesData.Cast<decimal>().Sum()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sales data");
                return Json(new { error = "Failed to load sales data" });
            }
        }

        [HttpGet("organizer/api/events-data")]
        public async Task<IActionResult> GetEventsData()
        {
            try
            {
                var organizerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
                var events = await _eventService.GetEventsByOrganizerAsync(organizerId);
                
                var eventData = events.Select(e => new
                {
                    id = e.Id,
                    title = e.Title,
                    eventDate = e.EventDate,
                    status = e.Status,
                    isPublished = e.IsPublished,
                    totalCapacity = e.TotalCapacity,
                    ticketsSold = e.Bookings?.Sum(b => b.Tickets?.Count ?? 0) ?? 0,
                    revenue = e.Bookings?.SelectMany(b => b.Tickets ?? new List<Ticket>())
                        .Where(t => t.Payment != null)
                        .Sum(t => t.Payment.Amount) ?? 0
                }).OrderByDescending(e => e.eventDate).ToList();

                return Json(eventData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting events data");
                return Json(new { error = "Failed to load events data" });
            }
        }

        [HttpGet("organizer/discounts")]
        public async Task<IActionResult> Discounts()
        {
            var organizerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var discounts = await _discountService.GetDiscountsByOrganizerAsync(organizerId);
            var availableEvents = await _discountService.GetAvailableEventsAsync(organizerId);
            
            var model = new DiscountViewModel
            {
                AvailableEvents = availableEvents.ToList()
            };

            ViewBag.Discounts = discounts;
            return View(model);
        }

        [HttpPost("organizer/discounts")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDiscount(DiscountViewModel model)
        {
            var organizerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Error: {error.ErrorMessage}");
                }

                model.AvailableEvents = (await _discountService.GetAvailableEventsAsync(organizerId)).ToList();
                var discounts = await _discountService.GetDiscountsByOrganizerAsync(organizerId);
                ViewBag.Discounts = discounts;
                ViewData["OpenCreateModal"] = true;
                return View("Discounts", model);
            }

            if (!model.EventId.HasValue)
            {
                ModelState.AddModelError("EventId", "Please select an event.");
            }

            var existingDiscount = await _discountService.GetDiscountByCodeAsync(model.Code);
            if (existingDiscount != null)
            {
                ModelState.AddModelError("Code", "This discount code already exists.");
            }

            if (model.EventId.HasValue)
            {
                var discountForEvent = await _discountService.GetDiscountByEventIdAsync(model.EventId.Value);
                if (discountForEvent != null)
                {
                    ModelState.AddModelError("EventId", "A discount already exists for the selected event. Each event can have only one discount.");
                }
            }

            if (!ModelState.IsValid)
            {
                model.AvailableEvents = (await _discountService.GetAvailableEventsAsync(organizerId)).ToList();
                var discounts = await _discountService.GetDiscountsByOrganizerAsync(organizerId);
                ViewBag.Discounts = discounts;
                ViewData["OpenCreateModal"] = true;
                return View("Discounts", model);
            }

            try
            {
                await _discountService.CreateDiscountAsync(model, organizerId);
            TempData["SuccessMessage"] = $"Discount code '{model.Code}' has been created successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while creating the discount. Please try again.";
            }
            
            return RedirectToAction("Discounts");
        }


        [HttpPost("organizer/discounts/edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDiscount(Guid id, DiscountViewModel model)
        {
            var organizerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state is invalid for discount edit. Errors: {Errors}", 
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));

                model.AvailableEvents = (await _discountService.GetAvailableEventsAsync(organizerId)).ToList();
                var discounts = await _discountService.GetDiscountsByOrganizerAsync(organizerId);
                ViewBag.Discounts = discounts;
                ViewData["OpenEditModal"] = id.ToString();
                return View("Discounts", model);
            }

            if (!model.EventId.HasValue)
            {
                ModelState.AddModelError("EventId", "Please select an event.");
            }

            var existingDiscount = await _discountService.GetDiscountByCodeAsync(model.Code);
            if (existingDiscount != null && existingDiscount.Id != id)
            {
                ModelState.AddModelError("Code", "This discount code already exists.");
            }

            if (model.EventId.HasValue)
            {
                var discountForEvent = await _discountService.GetDiscountByEventIdAsync(model.EventId.Value);
                if (discountForEvent != null && discountForEvent.Id != id)
                {
                    ModelState.AddModelError("EventId", "A discount already exists for the selected event. Each event can have only one discount.");
                }
            }

            if (!ModelState.IsValid)
            {
                model.AvailableEvents = (await _discountService.GetAvailableEventsAsync(organizerId)).ToList();
                var discounts = await _discountService.GetDiscountsByOrganizerAsync(organizerId);
                ViewBag.Discounts = discounts;
                ViewData["OpenEditModal"] = id.ToString();
                return View("Discounts", model);
            }

            try
            {
                await _discountService.UpdateDiscountAsync(id, model);
                TempData["SuccessMessage"] = $"Discount code '{model.Code}' has been updated successfully!";
                _logger.LogInformation("Discount {DiscountId} updated successfully by organizer {OrganizerId}", id, organizerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating discount {DiscountId} for organizer {OrganizerId}", id, organizerId);
                TempData["ErrorMessage"] = "An error occurred while updating the discount. Please try again.";
            }
            
            return RedirectToAction("Discounts");
        }

        [HttpPost("organizer/discounts/delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDiscount(Guid id)
        {
            var organizerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var discount = await _discountService.GetDiscountByIdAsync(id);
            
            if (discount == null)
            {
                TempData["ErrorMessage"] = "Discount not found or you don't have permission to delete it.";
                return RedirectToAction("Discounts");
            }

            try
            {
                await _discountService.DeleteDiscountAsync(id);
                TempData["SuccessMessage"] = "Discount has been deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the discount. Please try again.";
            }
            
            return RedirectToAction("Discounts");
        }

        [HttpPost("organizer/discounts/toggle/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleDiscountStatus(Guid id)
        {
            var organizerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var discount = await _discountService.GetDiscountByIdAsync(id);
            
            if (discount == null)
            {
                return Json(new { success = false, message = "Discount not found or you don't have permission to modify it." });
            }

            try
            {
                await _discountService.ToggleDiscountStatusAsync(id);
                return Json(new { success = true, message = "Discount status updated successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating discount status for discount {DiscountId}", id);
                return Json(new { success = false, message = "An error occurred while updating the discount status." });
            }
        }

        [HttpGet("organizer/discounts/get/{id}")]
        public async Task<IActionResult> GetDiscountData(Guid id)
        {
            var organizerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var discount = await _discountService.GetDiscountByIdAsync(id);
            
            if (discount == null)
            {
                return Json(new { success = false, message = "Discount not found or you don't have permission to access it." });
            }

            var model = new DiscountViewModel
            {
                Code = discount.Code,
                Type = discount.Type == "Percent" ? DiscountType.Percentage : DiscountType.FixedAmount,
                Value = discount.Value,
                EventId = discount.EventId,
                UsageLimit = discount.UsageLimit,
                ExpiryDate = discount.ValidTo,
                Description = discount.Description,
                IsActive = discount.IsActive
            };

            return Json(new { success = true, data = model });
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
                    EventPrices = eventEntity.Prices?.Select(p => new EventPriceViewModel
                    {
                        Category = p.Category,
                        Price = p.Price,
                        Stock = p.Stock,
                        IsActive = p.IsActive,
                        Description = p.Description,
                        PriceType = p.PriceType
                    }).ToList() ?? new List<EventPriceViewModel>()
                };

                // Ensure EventPrices is never null
                if (model.EventPrices == null)
                {
                    model.EventPrices = new List<EventPriceViewModel>();
                }

                // Process event image to convert S3 key to URL
                if (!string.IsNullOrEmpty(model.ImageUrl))
                {
                    model.ImageUrl = await _s3Service.GetImageUrlAsync(model.ImageUrl);
                }

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
                    model.EventPrices ??= new List<EventPriceViewModel>();
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
                model.EventPrices ??= new List<EventPriceViewModel>();
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

        [HttpPost("organizer/cancel-event/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelEvent(Guid id)
        {
            try
            {
                var organizerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
                var result = await _eventService.CancelEventAsync(id, organizerId);

                if (result)
                {
                    TempData["SuccessMessage"] = "Event has been cancelled successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Event not found, you don't have permission to cancel it, or it cannot be cancelled.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling event");
                TempData["ErrorMessage"] = "An error occurred while cancelling the event. Please try again.";
            }

            return RedirectToAction("Events");
        }

        [HttpPost("organizer/update-event-status/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateEventStatus(Guid id, string status)
        {
            try
            {
                var organizerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
                var result = await _eventService.UpdateEventStatusAsync(id, organizerId, status);

                if (result)
                {
                    TempData["SuccessMessage"] = $"Event status has been updated to {status} successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Event not found, you don't have permission to update it, or invalid status.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating event status");
                TempData["ErrorMessage"] = "An error occurred while updating the event status. Please try again.";
            }

            return RedirectToAction("Events");
        }
    }
}
