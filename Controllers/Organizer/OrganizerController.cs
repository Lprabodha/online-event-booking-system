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
            return View();
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
    }
}
