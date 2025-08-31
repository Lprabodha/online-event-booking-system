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
            var users = await _adminService.GetAllUsers();
            return View(users);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            var user = await _adminService.GetUserById(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
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
                var (succeeded, errors) = await _adminService.CreateUser(user, password, role);
                if (succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
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
            var user = await _adminService.GetUserById(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
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
                var result = await _adminService.UpdateUser(user);
                if (result)
                {
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(string.Empty, "Failed to update user.");
            }
            return View(user);
        }

        // POST: Admin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var result = await _adminService.SoftDeleteUser(id);
            if (!result)
            {
                return NotFound();
            }
            return RedirectToAction(nameof(Index));
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
    }
}
