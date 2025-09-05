using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using online_event_booking_system.Business.Interface;
using online_event_booking_system.Data.Entities;

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

        public async Task<IActionResult> Index()
        {
            try
            {
                var users = await _adminService.GetAllUsers();
                return View(users);
            }
            catch (Exception ex)
            {
                // Log the exception
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
        public async Task<IActionResult> Edit(string id)
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

        // POST: Admin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ApplicationUser user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }
            
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

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> ToggleStatus(string id)
        //{
        //    var result = await _adminService.ToggleUserStatus(id);
        //    if (!result)
        //    {
        //        return NotFound();
        //    }
        //    return RedirectToAction(nameof(Index));
        //}

        // Users Management
        public async Task<IActionResult> Users()
        {
            try
            {
                var users = await _adminService.GetAllUsers();
                return View(users);
            }
            catch (Exception ex)
            {
                // Log the exception
                return View(new List<ApplicationUser>());
            }
        }

        // Events Management
        public IActionResult Events()
        {
            // For now, return a view with sample data
            // In a real application, you would fetch events from a service
            return View();
        }

        // Organizers Management
        public IActionResult Organizers()
        {
            // For now, return a view with sample data
            // In a real application, you would fetch organizers from a service
            return View();
        }

        // Venues Management
        public IActionResult Venues()
        {
            // For now, return a view with sample data
            // In a real application, you would fetch venues from a service
            return View();
        }

        // Reports
        public IActionResult Reports()
        {
            // For now, return a view with sample data
            // In a real application, you would fetch report data from a service
            return View();
        }

        // Settings
        public IActionResult Settings()
        {
            // For now, return a view with sample data
            // In a real application, you would fetch settings from a service
            return View();
        }
    }
}
