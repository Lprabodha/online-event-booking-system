using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        
        public IActionResult Reports()
        {
            return View();
        }

        public IActionResult Payouts()
        {
            return View();
        }

        public IActionResult Support()
        {
            return View();
        }

        public IActionResult Settings()
        {
            return View();
        }

        public IActionResult Discounts()
        {
            return View();
        }

        public IActionResult CreateEvent()
        {
            return View();
        }

        public IActionResult EditEvent(int id)
        {
            return View();
        }
    }
}
