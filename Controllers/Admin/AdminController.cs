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

        /// <summary>
        /// Constructor for AdminController
        /// </summary>
        /// <param name="adminService"></param>
        /// <param name="userManager"></param>
        /// <param name="venueService"></param>
        public AdminController(IAdminService adminService, UserManager<ApplicationUser> userManager, IVenueService venueService)
        {
            _adminService = adminService;
            _userManager = userManager;
            _venueService = venueService;
        }

        /// <summary>
        /// Admin dashboard displaying all users
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// View details of a specific user by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Display form to create a new user
        /// </summary>
        /// <returns></returns>
        
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

        /// <summary>
        /// Load the edit organizer modal with user data
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> LoadEditOrganizerModal(string id)
        {
            var user = await _adminService.GetUserById(id);
            if (user == null)
            {
                return NotFound();
            }
            return PartialView("_EditOrganizerModal", user);
        }

        // POST: Admin/Edit/5

        /// <summary>
        /// Handle the submission of edited organizer details
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditOrganizer(ApplicationUser user)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.UserName))
                {
                    ModelState.AddModelError(string.Empty, "Email and Username are required.");
                    return PartialView("_EditOrganizerModal", user);
                }

                try
                {
                    var result = await _adminService.UpdateUser(user);
                    if (result)
                    {
                        TempData["SuccessMessage"] = "Organizer updated successfully.";
                        return Json(new { success = true, message = "Organizer updated successfully." });
                    }
                    ModelState.AddModelError(string.Empty, "Failed to update organizer.");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                }
            }
            return PartialView("_EditOrganizerModal", user);
        }

        // POST: Admin/Delete/5

        /// <summary>
        /// Soft delete a user by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Toggle a user's active status
        /// </summary>
        /// <param name="id"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id, int? page = null)
        {
            try
            {
                var user = await _adminService.GetUserById(id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction(nameof(Users), new { page = page ?? 1 });
                }

                var result = await _adminService.ToggleUserStatus(id);
                if (!result)
                {
                    TempData["ErrorMessage"] = "Failed to update user status.";
                    return RedirectToAction(nameof(Users), new { page = page ?? 1 });
                }

                var statusMessage = user.IsActive ? "User deactivated successfully." : "User activated successfully.";
                TempData["SuccessMessage"] = statusMessage;
                return RedirectToAction(nameof(Users), new { page = page ?? 1 });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while updating user status.";
                return RedirectToAction(nameof(Users), new { page = page ?? 1 });
            }
        }

        /// <summary>
        /// List all users with pagination
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>

        [HttpGet("admin/users")]
        public async Task<IActionResult> Users(int page = 1, int pageSize = 10)
        {
            try
            {
                var users = await _adminService.GetAllUsersWithRoles();
                
                // Calculate pagination
                var totalItems = users.Count();
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
                var skip = (page - 1) * pageSize;
                var paginatedUsers = users.Skip(skip).Take(pageSize).ToList();
                
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalItems = totalItems;
                ViewBag.StartItem = skip + 1;
                ViewBag.EndItem = Math.Min(skip + pageSize, totalItems);
                
                return View(paginatedUsers);
            }
            catch (Exception ex)
            {
                return View(new List<UserWithRoleViewModel>());
            }
        }

        /// <summary>
        /// Admin events management page
        /// </summary>
        /// <returns></returns>
        [HttpGet("admin/events")]
        public IActionResult Events()
        {
            return View();
        }

        [HttpGet("admin/organizers")]
        public async Task<IActionResult> Organizers(int page = 1, int pageSize = 6)
        {
            var organizers = await _adminService.GetUsersByRole("Organizer");
            
            var totalItems = organizers.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var skip = (page - 1) * pageSize;
            var paginatedOrganizers = organizers.Skip(skip).Take(pageSize).ToList();
            
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.StartItem = skip + 1;
            ViewBag.EndItem = Math.Min(skip + pageSize, totalItems);
            
            return View("Organizers", paginatedOrganizers);
        }

        /// <summary>
        /// Admin venues management page
        /// </summary>
        /// <returns></returns>
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
        public async Task<IActionResult> CreateOrganizer(ApplicationUser model)
        {
            if (ModelState.IsValid)
            {
                var (success, errors) = await _adminService.CreateOrganizer(model);
                if (success)
                {
                    TempData["SuccessMessage"] = "Organizer created successfully!";
                    return RedirectToAction(nameof(Organizers), new { page = 1 });
                }

                foreach (var error in errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            var validationErrors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            if (validationErrors.Any())
            {
                TempData["ValidationErrors"] = validationErrors;
            }

            var organizers = await _adminService.GetUsersByRole("Organizer");
            var totalItems = organizers.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / 6);
            var paginatedOrganizers = organizers.Take(6).ToList();
            
            ViewBag.CurrentPage = 1;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = 6;
            ViewBag.TotalItems = totalItems;
            ViewBag.StartItem = 1;
            ViewBag.EndItem = Math.Min(6, totalItems);
            
            return View("Organizers", paginatedOrganizers);
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
