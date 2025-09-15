using Microsoft.AspNetCore.Mvc;

namespace online_event_booking_system.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            switch (statusCode)
            {
                case 404:
                    ViewBag.ErrorMessage = "The page you are looking for cannot be found.";
                    ViewBag.StatusCode = statusCode;
                    break;
                case 500:
                    ViewBag.ErrorMessage = "An internal server error occurred.";
                    ViewBag.StatusCode = statusCode;
                    break;
                default:
                    ViewBag.ErrorMessage = "An error occurred while processing your request.";
                    ViewBag.StatusCode = statusCode;
                    break;
            }
            
            return View("NotFound");
        }

        [Route("Error")]
        public IActionResult Error()
        {
            ViewBag.ErrorMessage = "An unexpected error occurred.";
            ViewBag.StatusCode = 500;
            return View("NotFound");
        }

    }
}
