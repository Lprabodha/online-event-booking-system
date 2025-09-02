using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace online_event_booking_system.Controllers.Customer
{
    [Authorize(Roles = "Customer")]
    public class CustomerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
