using Microsoft.AspNetCore.Mvc;

namespace online_event_booking_system.Controllers.Organizer
{
    public class OrganizerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
