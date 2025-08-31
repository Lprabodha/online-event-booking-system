using Microsoft.AspNetCore.Mvc;

namespace online_event_booking_system.Controllers.Admin
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
