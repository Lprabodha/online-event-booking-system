using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace online_event_booking_system.Controllers.Customer
{
    [Authorize(Roles = "Customer")]
    public class CustomerController : Controller
    {
        [HttpGet("customer")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("customer/events")]
        public IActionResult Events()
        {
            return View();
        }

        [HttpGet("customer/bookings")]
        public IActionResult Bookings()
        {
            return View();
        }

        [HttpGet("customer/profile")]
        public IActionResult Profile()
        {
            return View();
        }

        [HttpGet("customer/support")]
        public IActionResult Support()
        {
            return View();
        }

        public IActionResult OrderDetails(int id)
        {
            // TODO: Fetch order details from database using the id
            // For now, return the view with sample data
            return View();
        }
    }
}