using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using online_event_booking_system.Models;

namespace online_event_booking_system.Controllers.Organizer
{
    [Authorize(Roles = "Organizer")]
    public class OrganizerController : Controller
    {
        [HttpGet("organizer")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("organizer/events")]
        public IActionResult Events()
        {
            return View();
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
        public IActionResult CreateEvent()
        {
            return View();
        }

        [HttpGet("organizer/edit-event/{id}")]
        public IActionResult EditEvent(int id)
        {
            return View();
        }

        private List<EventOption> GetAvailableEvents()
        {
            // In a real application, this would come from your database
            return new List<EventOption>
            {
                new EventOption { Id = 1, Name = "Summer Music Festival", EventDate = DateTime.Now.AddDays(30) },
                new EventOption { Id = 2, Name = "Tech Conference 2024", EventDate = DateTime.Now.AddDays(45) },
                new EventOption { Id = 3, Name = "Art Exhibition", EventDate = DateTime.Now.AddDays(60) },
                new EventOption { Id = 4, Name = "Food & Wine Tasting", EventDate = DateTime.Now.AddDays(75) },
                new EventOption { Id = 5, Name = "Comedy Night", EventDate = DateTime.Now.AddDays(90) }
            };
        }
    }
}
