using Microsoft.AspNetCore.Mvc;
using online_event_booking_system.Models;
using System.Diagnostics;
using online_event_booking_system.Business.Interface;
using online_event_booking_system.Data.Entities;

namespace online_event_booking_system.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ICategoryService _categoryService;
    private readonly IEventService _eventService;

    public HomeController(ILogger<HomeController> logger, ICategoryService categoryService, IEventService eventService)
    {
        _logger = logger;
        _categoryService = categoryService;
        _eventService = eventService;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            // Fetch active categories for the browse by category section
            var categories = (await _categoryService.GetActiveCategoriesAsync()).ToList();
            
            // Fetch upcoming and latest events
            var upcomingEvents = await _eventService.GetUpcomingEventsAsync(6);
            var latestEvents = await _eventService.GetLatestEventsAsync(4);
            
            // Create a view model to pass all data
            var viewModel = new HomePageViewModel
            {
                Categories = categories,
                UpcomingEvents = upcomingEvents,
                LatestEvents = latestEvents
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

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
