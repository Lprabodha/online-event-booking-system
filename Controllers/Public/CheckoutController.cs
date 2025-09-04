using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace online_event_booking_system.Controllers.Public
{
    public class CheckoutController : Controller
    {
        // [Authorize(Roles = "Customer")]
        [HttpGet("checkout/{id}")]
        public IActionResult Index(string id)
        {
            return View();
        }
    }
}
