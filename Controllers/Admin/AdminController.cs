using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using online_event_booking_system.Business.Interface;
using online_event_booking_system.Data.Entities;
using online_event_booking_system.Models.View_Models;

namespace online_event_booking_system.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(IAdminService adminService, UserManager<ApplicationUser> userManager)
        {
            _adminService = adminService;
            _userManager = userManager;
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
                // Validate required fields
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
            // Returns the partial view with the specific user object
            return PartialView("_EditOrganizerModal", user);
        }

        // POST: Admin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditOrganizer(ApplicationUser user)
        {
            if (ModelState.IsValid)
            {
                // Validate required fields
                if (string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.UserName))
                {
                    ModelState.AddModelError(string.Empty, "Email and Username are required.");
                    return View(user);
                }

                try
                {
                    var result = await _adminService.UpdateUser(user);
                    if (result)
                    {
                        TempData["SuccessMessage"] = "User updated successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    ModelState.AddModelError(string.Empty, "Failed to update user.");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                }
            }
            return View(user);
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


        // Users Management
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
        public IActionResult Venues()
        {
            return View();
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
        public async Task<IActionResult> CreateOrganizer(ApplicationUser model)
        {
            if (ModelState.IsValid)
            {
                var (success, errors) = await _adminService.CreateOrganizer(model);
                if (success)
                {
                    TempData["SuccessMessage"] = "Organizer created successfully!";
                    return RedirectToAction(nameof(Organizers)); // Redirect to the organizers list view
                }

                // If creation failed, add errors to ModelState to display in the view
                foreach (var error in errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If ModelState is invalid or creation failed, return to the view with errors
            // You might need to reload the page or handle this with AJAX
            return RedirectToAction(nameof(Organizers));
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
