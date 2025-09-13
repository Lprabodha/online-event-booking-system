using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using online_event_booking_system.Business.Interface;
using online_event_booking_system.Business.Service;
using online_event_booking_system.Data.Entities;
using online_event_booking_system.Models.View_Models;

namespace online_event_booking_system.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IVenueService _venueService;

        public AdminController(IAdminService adminService, UserManager<ApplicationUser> userManager, IVenueService venueService)
        {
            _adminService = adminService;
            _userManager = userManager;
            _venueService = venueService;
        }

        [HttpGet("admin")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var users = await _adminService.GetAllUsers();
                return View(users);
            }
            catch (Exception ex)
            {
                return View(new List<ApplicationUser>());
            }
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            
            try
            {
                var user = await _adminService.GetUserById(id);
                if (user == null)
                {
                    return NotFound();
                }
                return View(user);
            }
            catch
            {
                return NotFound();
            }
        }

        // GET: Admin/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ApplicationUser user, string password, string role)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.UserName) || string.IsNullOrEmpty(password))
                {
                    ModelState.AddModelError(string.Empty, "Email, Username, and Password are required.");
                    return View(user);
                }

                try
                {
                    var (succeeded, errors) = await _adminService.CreateUser(user, password, role);
                    if (succeeded)
                    {
                        TempData["SuccessMessage"] = "User created successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    foreach (var error in errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                }
            }
            return View(user);
        }

        // GET: Admin/Edit/5
        [HttpGet]
        public async Task<IActionResult> LoadEditOrganizerModal(string id)
        {
            var user = await _adminService.GetUserById(id);
            if (user == null)
            {
                return NotFound();
            }

            // Convert ApplicationUser to OrganizerViewModel
            var model = new OrganizerViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.ContactNumber,
                NIC = user.NIC ?? string.Empty,
                OrganizationName = user.OrganizationName ?? string.Empty,
                Address = user.Address ?? string.Empty,
                IsActive = user.IsActive
            };

            return PartialView("_EditOrganizerModal", model);
        }

        // POST: Admin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditOrganizer(OrganizerViewModel model)
        {
            Console.WriteLine("EditOrganizer method called");
            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState is not valid");
                TempData["ErrorMessage"] = "Please correct the validation errors and try again.";
                return RedirectToAction("Organizers", "Admin");
            }

            try
            {
                // Get existing user
                var existingUser = await _adminService.GetUserById(model.Id);
                if (existingUser == null)
                {
                    TempData["ErrorMessage"] = "Organizer not found.";
                    return RedirectToAction("Organizers", "Admin");
                }

                // Update user properties
                existingUser.FullName = model.FullName;
                existingUser.ContactNumber = model.PhoneNumber;
                existingUser.OrganizationName = model.OrganizationName;
                existingUser.Address = model.Address;
                existingUser.IsActive = model.IsActive;

                var result = await _adminService.UpdateUser(existingUser);
                if (result)
                {
                    Console.WriteLine("Organizer updated successfully, redirecting...");
                    TempData["SuccessMessage"] = "Organizer updated successfully!";
                    return RedirectToAction("Organizers", "Admin");
                }
                
                TempData["ErrorMessage"] = "Failed to update organizer. Please try again.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while updating the organizer: {ex.Message}";
            }

            return RedirectToAction("Organizers", "Admin");
        }

        // POST: Admin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var result = await _adminService.SoftDeleteUser(id);
                if (!result)
                {
                    return NotFound();
                }
                TempData["SuccessMessage"] = "User deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            var result = await _adminService.ToggleUserStatus(id);
            if (!result)
            {
                return NotFound();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("admin/users")]
        public async Task<IActionResult> Users()
        {
            try
            {
                var users = await _adminService.GetAllUsersWithRoles();
                return View(users);
            }
            catch (Exception ex)
            {
                return View(new List<UserWithRoleViewModel>());
            }
        }

        [HttpGet("admin/events")]
        public IActionResult Events()
        {
            return View();
        }

        [HttpGet("admin/organizers")]
        public async Task<IActionResult> Organizers()
        {
            var organizers = await _adminService.GetUsersByRole("Organizer");
            return View("Organizers", organizers);
        }

        [HttpGet("admin/venues")]
        public async Task<IActionResult> Venues()
        {
            var venues = await _venueService.GetAllVenuesAsync();
            return View(venues);
        }

        [HttpGet("admin/reports")]
        public IActionResult Reports()
        {
            return View();
        }

        [HttpGet("admin/settings")]
        public IActionResult Settings()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetUser(string id)
        {
            var userWithRole = await _adminService.GetUserWithRoleById(id);
            if (userWithRole == null)
            {
                return NotFound();
            }
            return Json(userWithRole);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrganizer(OrganizerViewModel model)
        {
            Console.WriteLine("CreateOrganizer method called");
            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState is not valid");
                TempData["ErrorMessage"] = "Please correct the validation errors and try again.";
                return RedirectToAction("Organizers", "Admin");
            }

            try
            {
                // Convert ViewModel to ApplicationUser
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    ContactNumber = model.PhoneNumber,
                    NIC = model.NIC,
                    Address = model.Address,
                    OrganizationName = model.OrganizationName,
                    IsActive = model.IsActive
                };

                var (success, errors) = await _adminService.CreateOrganizer(user);
                if (success)
                {
                    Console.WriteLine("Organizer created successfully, redirecting...");
                    TempData["SuccessMessage"] = "Organizer created successfully! An email with login credentials has been sent to the organizer.";
                    return RedirectToAction("Organizers", "Admin");
                }

                // Add validation errors to ModelState
                foreach (var error in errors ?? Enumerable.Empty<IdentityError>())
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                
                TempData["ErrorMessage"] = "Failed to create organizer. Please check the information and try again.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while creating the organizer: {ex.Message}";
            }

            return RedirectToAction("Organizers", "Admin");
        }

        [HttpGet]
        public async Task<IActionResult> GetOrganizerData(string id)
        {
            var user = await _adminService.GetUserById(id);
            if (user == null)
            {
                return NotFound();
            }
            return Json(user);
        }
    }
}
