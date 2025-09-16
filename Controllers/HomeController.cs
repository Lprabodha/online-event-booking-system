using Microsoft.AspNetCore.Mvc;
using online_event_booking_system.Models;
using System.Diagnostics;
using online_event_booking_system.Business.Interface;
using online_event_booking_system.Data.Entities;
using online_event_booking_system.Services;
using System.Linq;

namespace online_event_booking_system.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ICategoryService _categoryService;
    private readonly IEventService _eventService;
    private readonly IS3Service _s3Service;

    public HomeController(ILogger<HomeController> logger, ICategoryService categoryService, IEventService eventService, IS3Service s3Service)
    {
        _logger = logger;
        _categoryService = categoryService;
        _eventService = eventService;
        _s3Service = s3Service;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            // Fetch active categories for the browse by category section
            var categories = (await _categoryService.GetActiveCategoriesAsync()).ToList();
            
            // Fetch upcoming, latest, this week's, and next week's events
            var upcomingEvents = await _eventService.GetUpcomingEventsAsync(6);
            var latestEvents = await _eventService.GetLatestEventsAsync(4);
            var eventsThisWeek = await _eventService.GetEventsThisWeekAsync(6);
            var eventsNextWeek = await _eventService.GetEventsNextWeekAsync(6);
            
            // Process event images to convert S3 keys to URLs
            await ProcessEventImagesAsync(upcomingEvents);
            await ProcessEventImagesAsync(latestEvents);
            await ProcessEventImagesAsync(eventsThisWeek);
            await ProcessEventImagesAsync(eventsNextWeek);
            
            // Create a view model to pass all data
            var viewModel = new HomePageViewModel
            {
                Categories = categories,
                UpcomingEvents = upcomingEvents,
                LatestEvents = latestEvents,
                EventsThisWeek = eventsThisWeek,
                EventsNextWeek = eventsNextWeek
            };
            
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching data for home page");
            // Return empty data if there's an error
            return View(new HomePageViewModel
            {
                Categories = new List<Category>(),
                UpcomingEvents = new List<Event>(),
                LatestEvents = new List<Event>()
            });
        }
    }

    [HttpGet("privacy")]
    public IActionResult Privacy()
    {
        return View();
    }

    [HttpGet("faq")]
    public IActionResult Faq()
    {
        return View();
    }

    [HttpGet("contact")]
    public IActionResult Contact()
    {
        return View();
    }

    [HttpGet("events/filtered")]
    public async Task<IActionResult> FilteredEvents(string? search, string? category, string? dateFilter)
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
            
            // Process event images to convert S3 keys to URLs
            await ProcessEventImagesAsync(events);
            
            // Get categories for filter dropdown
            var categories = await _categoryService.GetActiveCategoriesAsync();
            
            // Pass filter values to view
            ViewBag.SearchTerm = search;
            ViewBag.SelectedCategory = category;
            ViewBag.SelectedDateFilter = dateFilter;
            ViewBag.Categories = categories;
            
            return View("~/Views/Events/Index.cshtml", events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching filtered events");
            return View("~/Views/Events/Index.cshtml", new List<Event>());
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private async Task ProcessEventImagesAsync(IEnumerable<Event> events)
    {
        foreach (var eventItem in events)
        {
            if (!string.IsNullOrEmpty(eventItem.Image))
            {
                // Use direct URL builder for correctness and performance (S3 or CDN)
                eventItem.Image = _s3Service.GetDirectUrl(eventItem.Image);
            }
        }
    }
}
