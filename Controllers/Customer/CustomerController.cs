using Microsoft.AspNetCore.Mvc;

namespace online_event_booking_system.Controllers.Customer
{
    public class CustomerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
