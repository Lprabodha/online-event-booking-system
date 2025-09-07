using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace online_event_booking_system.Controllers.Public
{
    public class EventsController : Controller
    {
        [AllowAnonymous]
        [HttpGet("events")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("events/{id}")]
        public IActionResult Details(Guid id)
        {
            return View();
        }
    }
}
