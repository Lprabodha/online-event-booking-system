using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using online_event_booking_system.Business.Interface;
using online_event_booking_system.Data.Entities;
using online_event_booking_system.Models;
using System.Security.Claims;

namespace online_event_booking_system.Controllers.Organizer
{
    [Authorize(Roles = "Organizer")]
    public class OrganizerController : Controller
    {
        private readonly IDiscountService _discountService;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrganizerController(IDiscountService discountService, UserManager<ApplicationUser> userManager)
        {
            _discountService = discountService;
            _userManager = userManager;
        }
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
        public async Task<IActionResult> Discounts()
        {
            var organizerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var discounts = await _discountService.GetDiscountsByOrganizerAsync(organizerId);
            var availableEvents = await _discountService.GetAvailableEventsAsync(organizerId);
            
            var model = new DiscountViewModel
            {
                AvailableEvents = availableEvents.ToList()
            };

            ViewBag.Discounts = discounts;
            return View(model);
        }

        [HttpPost("organizer/discounts")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDiscount(DiscountViewModel model)
        {
            var organizerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Error: {error.ErrorMessage}");
                }

                model.AvailableEvents = (await _discountService.GetAvailableEventsAsync(organizerId)).ToList();
                var discounts = await _discountService.GetDiscountsByOrganizerAsync(organizerId);
                ViewBag.Discounts = discounts;
                ViewData["OpenCreateModal"] = true;
                return View("Discounts", model);
            }

            if (!model.EventId.HasValue)
            {
                ModelState.AddModelError("EventId", "Please select an event.");
            }

            var existingDiscount = await _discountService.GetDiscountByCodeAsync(model.Code);
            if (existingDiscount != null)
            {
                ModelState.AddModelError("Code", "This discount code already exists.");
            }

            if (model.EventId.HasValue)
            {
                var discountForEvent = await _discountService.GetDiscountByEventIdAsync(model.EventId.Value);
                if (discountForEvent != null)
                {
                    ModelState.AddModelError("EventId", "A discount already exists for the selected event. Each event can have only one discount.");
                }
            }

            if (!ModelState.IsValid)
            {
                model.AvailableEvents = (await _discountService.GetAvailableEventsAsync(organizerId)).ToList();
                var discounts = await _discountService.GetDiscountsByOrganizerAsync(organizerId);
                ViewBag.Discounts = discounts;
                ViewData["OpenCreateModal"] = true;
                return View("Discounts", model);
            }

            try
            {
                await _discountService.CreateDiscountAsync(model, organizerId);
                TempData["SuccessMessage"] = $"Discount code '{model.Code}' has been created successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while creating the discount. Please try again.";
            }
            
            return RedirectToAction("Discounts");
        }


        [HttpPost("organizer/discounts/edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDiscount(Guid id, DiscountViewModel model)
        {
            var organizerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (!ModelState.IsValid)
            {
                Console.WriteLine("Model state is invalid:");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Error: {error.ErrorMessage}");
                }

                model.AvailableEvents = (await _discountService.GetAvailableEventsAsync(organizerId)).ToList();
                var discounts = await _discountService.GetDiscountsByOrganizerAsync(organizerId);
                ViewBag.Discounts = discounts;
                ViewData["OpenEditModal"] = id.ToString();
                return View("Discounts", model);
            }

            if (!model.EventId.HasValue)
            {
                ModelState.AddModelError("EventId", "Please select an event.");
            }

            var existingDiscount = await _discountService.GetDiscountByCodeAsync(model.Code);
            if (existingDiscount != null && existingDiscount.Id != id)
            {
                ModelState.AddModelError("Code", "This discount code already exists.");
            }

            if (model.EventId.HasValue)
            {
                var discountForEvent = await _discountService.GetDiscountByEventIdAsync(model.EventId.Value);
                if (discountForEvent != null && discountForEvent.Id != id)
                {
                    ModelState.AddModelError("EventId", "A discount already exists for the selected event. Each event can have only one discount.");
                }
            }

            if (!ModelState.IsValid)
            {
                model.AvailableEvents = (await _discountService.GetAvailableEventsAsync(organizerId)).ToList();
                var discounts = await _discountService.GetDiscountsByOrganizerAsync(organizerId);
                ViewBag.Discounts = discounts;
                ViewData["OpenEditModal"] = id.ToString();
                return View("Discounts", model);
            }

            try
            {
                await _discountService.UpdateDiscountAsync(id, model);
                TempData["SuccessMessage"] = $"Discount code '{model.Code}' has been updated successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while updating the discount. Please try again.";
            }
            
            return RedirectToAction("Discounts");
        }

        [HttpPost("organizer/discounts/delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDiscount(Guid id)
        {
            var organizerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var discount = await _discountService.GetDiscountByIdAsync(id);
            
            if (discount == null)
            {
                TempData["ErrorMessage"] = "Discount not found or you don't have permission to delete it.";
                return RedirectToAction("Discounts");
            }

            try
            {
                await _discountService.DeleteDiscountAsync(id);
                TempData["SuccessMessage"] = "Discount has been deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the discount. Please try again.";
            }
            
            return RedirectToAction("Discounts");
        }

        [HttpPost("organizer/discounts/toggle/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleDiscountStatus(Guid id)
        {
            var organizerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var discount = await _discountService.GetDiscountByIdAsync(id);
            
            if (discount == null)
            {
                return Json(new { success = false, message = "Discount not found or you don't have permission to modify it." });
            }

            try
            {
                await _discountService.ToggleDiscountStatusAsync(id);
                return Json(new { success = true, message = "Discount status updated successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while updating the discount status." });
            }
        }

        [HttpGet("organizer/discounts/get/{id}")]
        public async Task<IActionResult> GetDiscountData(Guid id)
        {
            var organizerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var discount = await _discountService.GetDiscountByIdAsync(id);
            
            if (discount == null)
            {
                return Json(new { success = false, message = "Discount not found or you don't have permission to access it." });
            }

            var model = new DiscountViewModel
            {
                Code = discount.Code,
                Type = discount.Type == "Percent" ? DiscountType.Percentage : DiscountType.FixedAmount,
                Value = discount.Value,
                EventId = discount.EventId,
                UsageLimit = discount.UsageLimit,
                ExpiryDate = discount.ValidTo,
                Description = discount.Description,
                IsActive = discount.IsActive
            };

            return Json(new { success = true, data = model });
        }

        [HttpGet("organizer/create-event")]
        public IActionResult CreateEvent()
        {
            return View();
        }

        [HttpGet("organizer/edit-event/{id}")]
        public IActionResult EditEvent(Guid id)
        {
            return View();
        }

    }
}
